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

namespace NetSocket
{   
    //容器用于存放接收的东西
    public class StateObject
    {
        public Socket workSocket = null;
        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
    }

    //先做好一主一从的socket类，一个server，只能监听一个客户端的信息，后面的人能连上，但是收不到消息
    
    public class ServerSocket
    {

        #region 静态成员声明部分
        //静态成员，所有函数都能访问(一开始是空，一旦onReceiveSocket被成功执行就有东西了)
        public Socket client_socket = null;
        public Socket server_socket = null;
        public string client_socket_info;

        //再加一个client状态信息，如果client一旦连接成功，则为true，一旦心跳检测失败，则为false
        public bool client_state = false;
        public System.Timers.Timer HeartbeatTimer;
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
        public void Create_Server(int ListenPort)
        {
            try
            {
                IPEndPoint ipep = new IPEndPoint(IPAddress.Any, ListenPort);
                server_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                server_socket.Bind(ipep);
                server_socket.Listen(10);
                //使用异步的方法，只要有新的连接，就执行回调函数(socketWatch是client的state)
                server_socket.BeginAccept(new AsyncCallback(AcceptCallback), server_socket);
                //MessageBox.Show("监听成功");
            }

            catch(Exception e)
            {
                //MessageBox.Show(e.ToString());
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
            catch(Exception CloseServerError)
            { 
                //这说明关不掉，说明还得保持(客户端关闭连接跟我关闭连接去执行一样的方法)
                HeartbeatTimer.Enabled = false;
                this.client_state = false;
                OnClientDisconnect(this.client_state);
            }


        }

        //BeginAccept触发之后，触发AcceptCallback
        public void AcceptCallback(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            client_socket = handler;
            client_socket_info = client_socket.RemoteEndPoint.ToString();

            //OnConnectionSuccess触发了，就把handler传递出去，至于谁要用，则看谁绑定了OnConnectionSuccess所委托的对象
            OnConnectionSuccess(handler);

            //设置client_state为真，表示客户端活了
            this.client_state = true;

            //Accept之后就开始心跳检测（1秒检测一次）
            this.HeartbeatTest(1000);

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

                //我们已经收到了来自客户端的信息，但是不一定是要显示在文本框，封装好的方法就应该是作为返回值传递出去，至于主程序要怎么用,这个类不关心
                OnReceiveSuccess(content);
            }
            catch(Exception ReceiveError)
            { 
                //不作处理就可以了,属于比较笨的办法
            }


            //收到完毕，再次准备接受，实现闭环委托链(这里只能用笨方法了，做个阀门，然后阀门关上了就不再接受数据，起始socket对象没有关闭)
            try
            {
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch(Exception ReceiveError)
            {
                //不作处理就可以了,属于比较笨的办法
            }

        }

        public void Send(Socket handler, String data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        public void SendCallback(IAsyncResult ar)
        {

            Socket handler = (Socket)ar.AsyncState;
            int bytesSent = handler.EndSend(ar);
            
        }

        //再给这个类添加一个心跳检测的功能（心跳检测类似于订阅，我既然不知道客户端什么时候会关闭，我只能每隔一定时间给客户端发送数据，用返回值来判断客户端是否关闭）
        //有人会问，你这个怎么又回到了定时器的时代呢？我的解释是，如果客户端也是你来写的，则你无须这么做，你只要在客户端关闭的代码中添加一行向服务器发送指定命令的代码，服务器收到这段特殊代码，就知道客户端关闭了
        //但是如果客户端不是你写的，则你不会知道客户端何时关闭。何况，这里的线程，对于主程序来说根本无需关心，我们还是把代码封装在类里，主程序还是只需要添加事件绑定而已。
        //心跳检测，需要传入一个心跳速度（每个多少时间检测一次）
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
            //给指定客户端发送数据（必须发送不可见字符）
            //string data = "";
            //byte[] byteData = Encoding.ASCII.GetBytes(data);
            byte[] byteData =new byte[]{11};
            
            //我这里在尝试向目标客户端发送数据
            try
            {
                this.client_socket.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client_socket);
            }
            //如果上面的try没有成功实现，下面就会报错了，但是我们不是需要报错，而是需要
            catch(Exception HeartbeatError)
            {
                //不作处理就可以了,属于比较笨的办法

                //设置client_state为假，表示客户端死了
                this.client_state = false;

                //注意这里很重要，HeartbeatTimer必须先停止，然后才能借助委托通知外部，否则被委托的方法将不知道timer已经停掉了，每秒执行一次
                //定时器也可以停掉了(在类中做全局变量，这跟面向对象是不冲突的，我只是希望这个变量让大家都能访问，传来传去多麻烦，放在公共位置，谁要用自己去用)
                HeartbeatTimer.Enabled = false;
                
                //跟前面连接成功一样，既然连接成功要委托ConnectionSuccess把消息传出去，这里也要把连接失败的消息传出去
                OnClientDisconnect(this.client_state);

                //断开连接了，就要继续保持监听，准备接收下一次的客户端连接
                try
                {
                    server_socket.BeginAccept(new AsyncCallback(AcceptCallback), server_socket);
                }
                catch(Exception HeartbearDisconnectError)
                { 
                
                }
                
            }

        }

    }



}
