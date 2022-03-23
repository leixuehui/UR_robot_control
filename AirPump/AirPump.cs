using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using LogManage;

namespace AirPump
{
    public class Air
    {
        public static Socket ClientSocket;
        public static IPAddress myIP;
        public static IPEndPoint ipe;
        public static void connect(int PORT)
        {
            ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            myIP = IPAddress.Parse("127.0.0.1");
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
        public static void init() 
        {
            sendcmd("Home","ALL");
        }
        public static void up()
        {

            string cmd = "setin";
            string io = "SuckerCylinderGroup";
            sendcmd(cmd, io);
        }
        public static void down()
        {

            string cmd = "setout";
            string io = "SuckerCylinderGroup";
            sendcmd(cmd, io);
        }
        public static void pressDown()
        {
            
            string cmd = "setout";
            string io = "PushPreeCylinderA";
            sendcmd(cmd, io);
            
            
        }
        public static void pressUp()
        {

            string cmd = "setin";
            string io = "PushPreeCylinderA";
            sendcmd(cmd, io);
        }
        public static void pickUp()
        {
            string cmd = "setout";
            string io = "VacuumValveGroup";
            sendcmd(cmd, io);
            
            while (true)
            {
                if (getStatus())
                    break;
            }
            
        }
        public static void drop()
        {
            string cmd = "setin";
            string io = "VacuumValveGroup";
            sendcmd(cmd, io);
        }
        public static bool getStatus()
        {
            string io1 = "VacuumSenseA1";
            string io2 = "VacuumSenseA2";
            string io3 = "VacuumSenseA3";
            string io4 = "VacuumSenseA4";
            string msgA1 = sendcmdAndRecive("GetValue", io1);
            string msgA2 = sendcmdAndRecive("GetValue", io2);
            string msgA3 = sendcmdAndRecive("GetValue", io3);
            string msgA4 = sendcmdAndRecive("GetValue", io4);
            Console.WriteLine(string.Format("A1={0},A2={1},A3={2},A4={3}",msgA1,msgA2,msgA3,msgA4));
            if (msgA1.Contains("OK") && msgA2.Contains("OK"))
                return true;
            
            
            return false;
            //Console.WriteLine(string.Format("status:{0}",msg));
        }
        public static string sendcmdAndRecive(string cmd, string io)
        {
            string command = cmd + "," + io;
            string data = "";
            byte[] buffersend = System.Text.Encoding.Default.GetBytes(command);//不开气泵则不能执行，加保护
            //如果ClientSocket被关闭则不继续运行
            if (ClientSocket != null && ClientSocket.Connected == true)
            {
                try
                {
                    ClientSocket.Send(buffersend);
                    byte[] bytes = new byte[1024];
                    int bytesRec = ClientSocket.Receive(bytes);
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, bytesRec);
                }
                catch (Exception e)
                {
                    Log.E("AirPumpException", "气泵连接已终止");
                    MessageBox.Show("气泵连接已终止");
                }
            }
            else
            {
                Log.E("AirPump", "气泵连接失败");
            }
            return data;
        }
        public static void sendcmd(string cmd, string io)
        {
            string command = cmd + "," + io;

            byte[] buffersend = System.Text.Encoding.Default.GetBytes(command);//不开气泵则不能执行，加保护
            //如果ClientSocket被关闭则不继续运行
            if (ClientSocket != null && ClientSocket.Connected == true)
            {
                try
                {
                    ClientSocket.Send(buffersend);
                    byte[] bytes = new byte[1024];
                }
                catch(Exception e)
                {
                    Log.E("AirPumpException", "气泵连接已终止");
                    MessageBox.Show("气泵连接已终止");
                }

                //int bytesRec = ClientSocket.Receive(bytes);

                //string data = System.Text.Encoding.ASCII.GetString(bytes, 0, bytesRec);
            }
            else
            {
                Log.E("AirPump", "气泵连接失败");
            }
        }
        public static void close()
        {
            string cmd = "setin";
            string io = "VacuumValveGroup";
            sendcmd(cmd, io);//停止吸气
            if(ClientSocket != null && ClientSocket.Connected == true)
            {
                ClientSocket.Shutdown(SocketShutdown.Both);
                ClientSocket.Close();
                ClientSocket.Dispose();//释放socket端口
                ClientSocket = null;
            }
            
        }
    }
}
