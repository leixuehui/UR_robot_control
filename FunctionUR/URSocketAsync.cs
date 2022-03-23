using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace URSocketTest
{
    //容器用于存放接收的东西
    public class StateObject
    {
        public Socket workSocket = null;
        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
    }

    public class ServerSocket
    {

        #region 静态成员声明部分
        //所有函数都能访问(一开始是空，一旦onReceiveSocket被成功执行就有东西了)
        public Socket server_socket = null;
        public Socket client_socket = null;
        public Socket listener;
        public Socket handler;
        public int server_listenPort;
        public string client_socket_info;
        public string client_socket_ip;

        //再加一个client状态信息，如果client一旦连接成功，则为true，一旦心跳检测失败，则为false
        public bool client_state = false;
        public System.Timers.Timer HeartbeatTimer;

        //声明起始时间和终止时间
        public DateTime dtnow = DateTime.Now;
        public DateTime dtend = DateTime.Now;

        #endregion

        #region 委托声明部分

        //把自己的AcceptCallback方法委托给ConnectionSuccess，让他去告诉别人我收到客户端了
        public delegate void ConnectionSuccess(Socket SocketName);

        //把这个委托的触发条件指定为OnConnectionSuccess事件，如果我手动执行类OnConnectionSuccess，则认为ConnectionSuccess需要被告知，告知的内容就是Socket SocketName（客户端的socket信息）
        public event ConnectionSuccess OnConnectionSuccess;

        //把自己的ReceiveCallback方法委托给ReceiveSuccess，让他去告诉别人我收到来自客户端的数据了
        public delegate void ReceiveSuccess(string clientMessage);

        //把这个委托的触发条件指定为OnReceiveSuccess，如果我手动执行OnReceiveSuccess，则认为ReceiveSuccess需要被告知，告知的内容就是string clientMessage（客户端发来的消息）
        public event ReceiveSuccess OnReceiveSuccess;

        //把自己的HeartbeatTestResult方法委托给ClientDisconnect，让他去告诉别人我检测到客户端死掉了
        public delegate void ClientDisconnect(bool ConnectState);

        //把这个委托的触发条件指定为OnClientDisconnect，如果我手动执行OnClientDisconnect，则认为ClientDisconnect需要被告知，告知的内容就是bool ConnectState（客户端已经死亡的消息）
        public event ClientDisconnect OnClientDisconnect;

        #endregion

        //自己的生成serverSocket的方法，来监听自己的某个端口
        public void Create_Server()
        {
            try
            {
                IPEndPoint ipep = new IPEndPoint(IPAddress.Any, server_listenPort);
                server_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                server_socket.Bind(ipep);
                server_socket.Listen(10);
                server_socket.SendTimeout = 1000;
                server_socket.ReceiveTimeout = 200;
                //使用异步的方法，只要有新的连接，就执行回调函数(socketWatch是client的state)
                server_socket.BeginAccept(new AsyncCallback(AcceptCallback), server_socket);
                //MessageBox.Show("监听成功");
            }

            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

        }

        //关闭自己socket的方法，直接就是关闭客户端连接（一对一，所以一定知道客户端是谁）
        public void Close_Server()
        {
            //这里把server_socket关掉了还是没反应，但是把client_socket关掉就立即有反应了(SocketShutdown.Both除了both还可以区分receive和send)
            try
            {
                this.client_socket.Shutdown(SocketShutdown.Both);
                this.client_socket.Close();

            }
            catch (Exception CloseServerError)
            {
                //这说明关不掉，说明还得保持(客户端关闭连接跟我关闭连接去执行一样的方法)
                try
                {
                    HeartbeatTimer.Enabled = false;
                    this.client_state = false;
                    OnClientDisconnect(this.client_state);
                }
                catch { }

            }


        }

        //BeginAccept触发之后，触发AcceptCallback
        public void AcceptCallback(IAsyncResult ar)
        {
            listener = (Socket)ar.AsyncState;
            handler = listener.EndAccept(ar);

            client_socket = handler;
            client_socket_info = client_socket.RemoteEndPoint.ToString();

            string[] temp = client_socket_info.Split(new char[] { ':' });
            client_socket_ip = temp[0];

            //OnConnectionSuccess触发了，就把handler传递出去，至于谁要用，则看谁绑定了OnConnectionSuccess所委托的对象
            OnConnectionSuccess(handler);

            //有了Client连接，就初始化定时器
            dtnow = DateTime.Now;
            this.HeartbeatTest(1000);

            //设置client_state为真，表示客户端活了
            this.client_state = true;

            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
        }

        //AcceptCallback触发之后，触发ReceiveCallback，又回过来执行BeginReceive
        public void ReceiveCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            try
            {
                int bytesRead = handler.EndReceive(ar);
                //清空StringBuilder的方法
                state.sb.Remove(0, state.sb.Length);
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                string content = state.sb.ToString();

                //注意，这里得到了来自Client的信息，我并不急着把东西传递出去，而是先判定传过来的是什么内容
                if (content == "Heartbeat")
                {
                    //刷新起始时间(收到一次Heartbeat，就认为起始时间为最新)，同时也不发送OnReceiveSuccess信息
                    dtnow = DateTime.Now;
                }
                else
                {
                    OnReceiveSuccess(content);
                }


            }
            catch (Exception ReceiveError){}


            //收到完毕，再次准备接受，实现闭环委托链(这里只能用笨方法了，做个阀门，然后阀门关上了就不再接受数据，起始socket对象没有关闭)
            try
            {
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception ReceiveError){}

        }

        public void Send(Socket handler, String data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        public void SendSync(Socket handler, String data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            handler.Send(byteData);
        }

        public void SendCallback(IAsyncResult ar)
        {
            Socket handler = (Socket)ar.AsyncState;
            int bytesSent = handler.EndSend(ar);
        }

        //当收到连接客户端的时候，初始化这个计时器（只会在Connect之后执行一次）
        public void HeartbeatTest(int HeartRate)
        {
            //声明定时器，每隔一定时间做检测，注意这个定时器一定要设置AutoReset和Enabled属性才能正确启动
            HeartbeatTimer = new System.Timers.Timer(HeartRate);
            HeartbeatTimer.Elapsed += new System.Timers.ElapsedEventHandler(HeartbeatTestResult);
            HeartbeatTimer.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；
            HeartbeatTimer.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
        }

        //把心跳检测的结果通过OnClientDisconnect委托发布出去
        public void HeartbeatTestResult(object source, System.Timers.ElapsedEventArgs e)
        {
            //不管能不能收到来自于Client的信息，我必须有一个双向的心跳包，即便Client跟我断开连接，我也要一直向他发送数据
            //如果没有双向的心跳包，则这个活动的socket连接将只能维持一次，下次即便Client再发送数据，这里也收不到了
            //byte[] byteData = new byte[] { 11 };
            byte[] byteData = System.Text.Encoding.Default.GetBytes("Heartbeat");


            try
            {
                this.client_socket.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client_socket);
            }
            catch (Exception HeartbeatError)
            {
                //设置client_state为假，表示客户端死了
                this.client_state = false;
                HeartbeatTimer.Enabled = false;
                OnClientDisconnect(this.client_state);

                //断开连接了，就要继续保持监听，准备接收下一次的客户端连接
                try
                {
                    server_socket.BeginAccept(new AsyncCallback(AcceptCallback), server_socket);
                }
                catch { }
            }

            //获取系统当前时间为终止时间，并计算两个时间差，看是否在容许的范围之内
            dtend = DateTime.Now;
            TimeSpan dtDiff = dtend - dtnow;
            if (dtDiff.TotalMilliseconds > 3000)
            {
                this.client_state = false;
                HeartbeatTimer.Enabled = false;
                OnClientDisconnect(this.client_state);

                //断开连接了，就要继续保持监听，准备接收下一次的客户端连接
                try
                {
                    server_socket.BeginAccept(new AsyncCallback(AcceptCallback), server_socket);
                }
                catch { }
            }
        }

    }



}
