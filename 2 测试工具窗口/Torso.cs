using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

using URSocketTest;
using URControl;


namespace UR_点动控制器
{
    public partial class Torso : Form
    {
        public Torso()
        {
            InitializeComponent();
        }
        //创建一个Server对象(用来测试与实际的两个UR的Client通信是否成功)
        ServerSocket ServerA;
        ServerSocket ServerB;

        URControlHandle URControllerA;
        URControlHandle URControllerB;

        //把界面的所有量都做成全局的
        //其中Server是对应两个客户端的
        public string ServerIP;
        public int ServerPortA;
        public int ServerPortB;

        public string ClientAIP;
        public int ClientAPort;
        public string ClientBIP;
        public int ClientBPort;

        //设置一些监控AB机器是否到位的布尔值
        public static bool State_RobotA_Prepare_1;
        public static bool State_RobotB_Prepare_1;

        public static bool State_RobotA_Job1;
        public static bool State_RobotB_Job1;

        public static bool State_RobotA_Job2;
        public static bool State_RobotB_Job2;

        public static bool State_RobotA_Job3;
        public static bool State_RobotB_Job3;

        public static AutoResetEvent mEventPrepare1 = new AutoResetEvent(false);
        public static AutoResetEvent mEventJob1 = new AutoResetEvent(false);
        public static AutoResetEvent mEventJob2 = new AutoResetEvent(false);
        public static AutoResetEvent mEventJob3 = new AutoResetEvent(false);  

        //初始化按钮
        private void btnInitial_Click(object sender, EventArgs e)
        {
            ServerA = new ServerSocket();
            ServerB = new ServerSocket();

            URControllerA = new URControlHandle();
            URControllerB = new URControlHandle();

            //按钮的可用和不可用确保只会初始化一次
            this.btnInitial.Enabled = false;
            this.btnReset.Enabled = true;

            ServerIP = txtIPAddressPC.Text;
            ServerA.server_listenPort = Convert.ToInt32(txtPortPCToRobotA.Text);
            ServerB.server_listenPort = Convert.ToInt32(txtPortPCToRobotB.Text);

            ClientAIP = txtIPAddressRobotA.Text;
            ClientAPort = Convert.ToInt32(txtPortRobotA.Text);

            ClientBIP = txtIPAddressRobotB.Text;
            ClientBPort = Convert.ToInt32(txtPortRobotB.Text);

            //AB两台机器分开
            try
            {
                ServerA.Create_Server();
                ServerA.OnConnectionSuccess += new ServerSocket.ConnectionSuccess(ConnectOverA);
                ServerA.OnReceiveSuccess += new ServerSocket.ReceiveSuccess(ReceiveOverA);
                ServerA.OnClientDisconnect += new ServerSocket.ClientDisconnect(DisconnectOverA);

                URControllerA.Creat_client(ClientAIP, ClientAPort);
            }
            catch(Exception ConnectError)
            {
                MessageBox.Show("无法连接A机器" + ConnectError.ToString());
            }

            try
            {
                ServerB.Create_Server();
                ServerB.OnConnectionSuccess += new ServerSocket.ConnectionSuccess(ConnectOverB);
                ServerB.OnReceiveSuccess += new ServerSocket.ReceiveSuccess(ReceiveOverB);
                ServerB.OnClientDisconnect += new ServerSocket.ClientDisconnect(DisconnectOverB);

                URControllerB.Creat_client(ClientBIP, ClientBPort);
            }
            catch (Exception ConnectError)
            {
                MessageBox.Show("无法连接B机器" + ConnectError.ToString());
            }

        }

        //重置监听
        private void btnReset_Click(object sender, EventArgs e)
        {
            this.btnInitial.Enabled = true;
            this.btnReset.Enabled = false;

            ServerA.Close_Server();
            ServerA = null;
            ServerB.Close_Server();
            ServerB = null;
        }

        private void btnTestRobotA_Click(object sender, EventArgs e)
        {
            string str = txtRobotACommand.Text;

            //这里用同步的或者异步的都可以
            //ServerA.SendSync(ServerA.client_socket, str);
            ServerA.Send(ServerA.client_socket,str);
        }

        private void btnTestRobotB_Click(object sender, EventArgs e)
        {
            string str = txtRobotBCommand.Text;

            //这里用同步的或者异步的都可以
            ServerB.Send(ServerB.client_socket, str);
        }

        private void Torso_Load(object sender, EventArgs e)
        {
            this.btnInitial.Enabled = true;
            this.btnReset.Enabled = false;
        }

        //如果Server创建之后得到客户端的连接
        void ConnectOverA(Socket o)
        {
            listBoxFromRobotA.Items.Clear();
            listBoxFromRobotA.Items.Add("来自RobotA：" + o.RemoteEndPoint.ToString());
        }

        void ConnectOverB(Socket o)
        {
            listBoxFromRobotB.Items.Clear();
            listBoxFromRobotB.Items.Add("来自RobotB：" + o.RemoteEndPoint.ToString());
        }

        //如果Server创建之后得到客户端的反馈值
        void ReceiveOverA(string Message)
        {
            UpdateSignalRobotA(Message);
            listBoxFromRobotA.Items.Add(Message);
        }

        void ReceiveOverB(string Message)
        {
            UpdateSignalRobotB(Message);
            listBoxFromRobotB.Items.Add(Message);

        }

        //如果Server创建之后客户端断开连接
        void DisconnectOverA(bool ClientState)
        {
                listBoxFromRobotA.Items.Add("RobotA已断开！");

        }

        //如果Server创建之后客户端断开连接
        void DisconnectOverB(bool ClientState)
        {
                listBoxFromRobotB.Items.Add("RobotB已断开！");
        }

        private void btnClearFeedA_Click(object sender, EventArgs e)
        {
                listBoxFromRobotA.Items.Clear();
        }

        private void btnClearFeedB_Click(object sender, EventArgs e)
        {
                listBoxFromRobotB.Items.Clear();
        }

        //AB两点原点的取得，就在主界面分别连接两台UR，然后显示自定义命令即可
        private void btnRobotAGoZeroPoint_Click(object sender, EventArgs e)
        {
            //本来想从30002端口发送让他们回原点的命令，这里既然是在UR机器上运行程序，就还是通过29999发送Play命令
            /*
            string str = "movel(p[0.5555,0.0223,-0.1106,-1.1420,2.1780,0.6530], a = 0.15, v = 0.15)";
            URControllerA.Send_command(str);
            */
            string Feedback = URControllerA.Send_command_WithFeedback("play");
            listBoxFromRobotA.Items.Add(Feedback);

        }

        private void btnRobotBGoZeroPoint_Click(object sender, EventArgs e)
        {
             /*
            string str = "movel(p[-0.3833,-0.3916,-0.0850,-0.2380,-2.3450,-0.2950], a = 0.15, v = 0.15)";
            URControllerB.Send_command(str);
            */

            string Feedback = URControllerB.Send_command_WithFeedback("play");
            listBoxFromRobotB.Items.Add(Feedback);

        }

        private void btnRobotAStop_Click(object sender, EventArgs e)
        {
            string Feedback = URControllerA.Send_command_WithFeedback("stop");
            listBoxFromRobotA.Items.Add(Feedback);
        }

        private void btnRobotBStop_Click(object sender, EventArgs e)
        {
            string Feedback = URControllerB.Send_command_WithFeedback("stop");
            listBoxFromRobotB.Items.Add(Feedback);
        }

        private void btnRunJob1_Click(object sender, EventArgs e)
        {
            string str = "job1";
            ServerA.Send(ServerA.client_socket, str);
            ServerB.Send(ServerB.client_socket, str);

        }

        private void btnRunjob2_Click(object sender, EventArgs e)
        {
            string str = "job2";
            ServerA.Send(ServerA.client_socket, str);
            ServerB.Send(ServerB.client_socket, str);

        }

        private void btnRunjob3_Click(object sender, EventArgs e)
        {
            string str = "job3";
            ServerA.Send(ServerA.client_socket, str);
            ServerB.Send(ServerB.client_socket, str);

        }

        //连续运行Job1和Job2和Job3(可以测试把速度改小，看是否等这个事情做完了才开始一起做下一件事情)
        private void btnRunjob123_Click(object sender, EventArgs e)
        {
            //首先要重置所有信号
            ResetAllSignals();

            //其次让两台UR都停止运动
            btnRobotAStop.PerformClick();
            btnRobotBStop.PerformClick();
            MessageBox.Show("已停止运行");

            //启动两个子线程，一个用来检测当前两台机器人的状态，另一个用来根据检测到的信号执行具体的任务
            Thread threadCheckSignal = new Thread(new ThreadStart(threadCheckSignalMethod));
            threadCheckSignal.Start();

            Thread threadDoWork = new Thread(new ThreadStart(threadDoWorkMethod));
            threadDoWork.Start();

            //然后让两台UR都开始运行
            btnRobotAGoZeroPoint.PerformClick();
            btnRobotBGoZeroPoint.PerformClick();
            MessageBox.Show("已开始运行");

        }

        //首先主线程会不断的得到AB两台机器的运行状态（根据得到的状态修改四组监控的布尔值）
        void threadCheckSignalMethod()
        {
            while(true)
            {
                //如果两个Prepare都好了
                if (State_RobotA_Prepare_1 && State_RobotB_Prepare_1)
                {
                    mEventPrepare1.Set();
                }
                else if (State_RobotA_Job1 && State_RobotB_Job1)
                {
                    mEventJob1.Set();
                }
                else if (State_RobotA_Job2 && State_RobotB_Job2)
                {
                    mEventJob2.Set();
                }
                else if (State_RobotA_Job3 && State_RobotB_Job3)
                {
                    mEventJob3.Set();
                }

            }
        }

        void threadDoWorkMethod()
        {
            string str = "";

            /********************************************************************************/
            //在子线程中产生阻滞(等两台UR都走到了Prepare点)
            mEventPrepare1.WaitOne();

            //重置信号
            ResetAllSignals();

            str = "job1";
            ServerA.Send(ServerA.client_socket, str);
            ServerB.Send(ServerB.client_socket, str);

            while (true)
            {
                /********************************************************************************/
                //在子线程中产生阻滞(等Job1执行完毕)4
                mEventJob1.WaitOne();

                //重置信号
                ResetAllSignals();

                str = "job2";
                ServerA.Send(ServerA.client_socket, str);
                ServerB.Send(ServerB.client_socket, str);

                /********************************************************************************/
                //在子线程中产生阻滞(等Job2执行完毕)
                mEventJob2.WaitOne();

                //重置信号
                ResetAllSignals();

                str = "job3";
                ServerA.Send(ServerA.client_socket, str);
                ServerB.Send(ServerB.client_socket, str);

                /********************************************************************************/
                //在子线程中产生阻滞(等Job3执行完毕)
                mEventJob3.WaitOne();

                //重置信号
                ResetAllSignals();

                str = "job1";
                ServerA.Send(ServerA.client_socket, str);
                ServerB.Send(ServerB.client_socket, str);
            }

        }

        //根据从A机器得到的信号更新A的信号源
        void UpdateSignalRobotA(string SignalFromRobot)
        {
            if (SignalFromRobot.Contains("Prepare1 done"))
            {
                //MessageBox.Show("Prepare1 done");
                State_RobotA_Prepare_1 = true;
            }
            else if (SignalFromRobot.Contains("job1 done"))
            {
                //MessageBox.Show("job1 done");
                State_RobotA_Job1 = true;
            }
            else if (SignalFromRobot.Contains("job2 done"))
            {
                //MessageBox.Show("job2 done");
                State_RobotA_Job2 = true;

            }
            else if (SignalFromRobot.Contains("job3 done"))
            {
                //MessageBox.Show("job3 done");
                State_RobotA_Job3 = true;

            }
        }

        //根据从B机器得到的信号更新信号源
        void UpdateSignalRobotB(string SignalFromRobot)
        {
            if (SignalFromRobot.Contains("Prepare1 done"))
            {
                //MessageBox.Show("Prepare1 done");
                State_RobotB_Prepare_1 = true;
            }
            else if (SignalFromRobot.Contains("job1 done"))
            {
                //MessageBox.Show("job1 done");
                State_RobotB_Job1 = true;
            }
            else if (SignalFromRobot.Contains("job2 done"))
            {
                //MessageBox.Show("job2 done");
                State_RobotB_Job2 = true;

            }
            else if (SignalFromRobot.Contains("job3 done"))
            {
                //MessageBox.Show("job3 done");
                State_RobotB_Job3 = true;

            }
        }

        //重置AB两台机器的运行状态
        void ResetAllSignals()
        {
            State_RobotA_Prepare_1 = false;
            State_RobotB_Prepare_1 = false;

            State_RobotA_Job1 = false;
            State_RobotB_Job1 = false;

            State_RobotA_Job2 = false;
            State_RobotB_Job2 = false;

            State_RobotA_Job3 = false;
            State_RobotB_Job3 = false;

            mEventPrepare1.Reset();
            mEventJob1.Reset();
            mEventJob2.Reset();
            mEventJob3.Reset();
        }




    }
}
