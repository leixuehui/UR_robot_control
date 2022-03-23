using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

//调用外部类
using ModbusCommunication;
using URDate;


namespace URControl
{
    class URControlHandle
    {

        public Socket ClientSocket;
        public IPAddress myIP;
        public IPEndPoint ipe;

        //创建只需要实例化这个socket即可，不需要连接
        public void Creat_client(string IP, int PORT)
        {
            //所有UR的控制都在这里完成，先生成一个ClientSocket
            ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            myIP = IPAddress.Parse(IP);
            ipe = new IPEndPoint(myIP, PORT);

            //一次连接终生使用
            try
            {
                ClientSocket.Connect(ipe);
            }

            catch (Exception connectError)
            {
                MessageBox.Show(connectError.ToString());

            }

        }

        //因为连接是同步的方法，会导致阻塞，所以把连接功能放到与发送一起执行
        public void Send_command(string command)
        {
            command += "\r\n";
            byte[] buffersend = System.Text.Encoding.Default.GetBytes(command);

            try
            {
                ClientSocket.Send(buffersend);
            }
            catch (Exception sendError)
            {
                //MessageBox.Show(sendError.ToString());
                //这就是说明没有连接到UR(实际操作中，即便连接正常了，也会有这样的问题)
                MessageBox.Show("未取得与UR的连接，请确认连接正常。");

            }

        }
		private bool arrive(double px1, double py1, double pz1, double pu1, double pv1, double pw1, double px2, double py2, double pz2, double pu2, double pv2, double pw2)
        {
            double value = 0.003;//0.001不行
            double x = System.Math.Abs(px1 - px2);
            double y = System.Math.Abs(py1 - py2);
            double z = System.Math.Abs(pz1 - pz2);
            double u = System.Math.Abs(pu1 - pu2);
            double v = System.Math.Abs(pv1 - pv2);
            double w = System.Math.Abs(pw1 - pw2);

            if (System.Math.Abs(px1 - px2) < value && System.Math.Abs(py1 - py2) < value && System.Math.Abs(pz1 - pz2) < value
                && System.Math.Abs(pu1 - pu2) < value && System.Math.Abs(pv1 - pv2) < value && System.Math.Abs(pw1 - pw2) < value)
                return true;
            return false;
        }

        //前面发送的指令都是单向的，但是有的指令是请求UR提供返回值的
        public string Send_command_WithFeedback(string command)
        {
            command += "\r\n";
            byte[] buffersend = System.Text.Encoding.Default.GetBytes(command);
			double dst_x = 0.0, dst_y = 0.0, dst_z = 0.0, dst_u = 0.0, dst_v = 0.0, dst_w = 0.0;
            try
            {
                ClientSocket.Send(buffersend);
            }
            catch (Exception sendError)
            {
                //MessageBox.Show(sendError.ToString());
				Console.WriteLine(sendError.ToString());
            }

             //发送完了之后立即等待接收
            byte[] bytes = new byte[1024];
            string data = "";
            int bytesRec = 0;
            if (ClientSocket.Connected==true)
                bytesRec = ClientSocket.Receive(bytes);

            data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
			
			if (command.Contains("movel(p["))
            {
                string[] p = command.Split(new char[2] { '[', ']' });
                string[] q = p[1].Split(new char[] {','});
                dst_x = double.Parse(q[0]); 
                dst_y = double.Parse(q[1]); 
                dst_z = double.Parse(q[2]); 
                dst_u = double.Parse(q[3]); 
                dst_v = double.Parse(q[4]); 
                dst_w = double.Parse(q[5]); 
                while (true)
                {
                    //Console.WriteLine(string.Format("arrive ({0},{1},{2},{3},{4},{5})", URDateHandle.Positions_X, URDateHandle.Positions_Y, URDateHandle.Positions_Z, URDateHandle.Positions_U, URDateHandle.Positions_V, URDateHandle.Positions_W));
                    if (arrive(dst_x, dst_y, dst_z, dst_u, dst_v, dst_w, URDateHandle.Positions_X, URDateHandle.Positions_Y, URDateHandle.Positions_Z, URDateHandle.Positions_U, URDateHandle.Positions_V, URDateHandle.Positions_W) == true)
                        break;
                }
            }
            return data;
        }

        //还有最极端的一种情况，我刚连接到Dashboard的时候，Dashboard会主动给我发送一条命令
        public string No_command_WaitFeedback()
        {
            byte[] bytes = new byte[1024];
            string data = "";
            int bytesRec = ClientSocket.Receive(bytes);

            data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
            return data;
        }

        //只有在点击退出按钮才关闭
        public void Close_client()
        {
            try
            {
                if(ClientSocket != null)
                {
                    ClientSocket.Shutdown(SocketShutdown.Both);
                    ClientSocket.Close();
                }
                
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

        }


    }
}
