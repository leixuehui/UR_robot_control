using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;
using System.Threading;

//ModbusTCP通讯类说明
/*
传统的Socket类
//初始化没有initialServer方法，也没有在线程中keepConnectServer方法

Write方法：
1 connectServer
2 凑数据(sendData.Add)
3 发数据(socket.Send(sendData.ToArray());)

Read方法：
1 connectServer
2 凑数据(sendData.Add)
3 发数据(socket.Send(sendData.ToArray());)
4 接受数据(socket.Receive(ReadBufferData);)
5 主动关闭(this.closeServer())

更快的Socket类
//初始化要做initialServer方法，并在线程中keepConnectServer方法
 * 
Write方法：
1 凑数据(sendData.Add)
2 发数据(socket.Send(sendData.ToArray());)

Read方法：
1 凑数据(sendData.Add)
2 发数据(socket.Send(sendData.ToArray());)
3 接受数据(socket.Receive(ReadBufferData);)
 * 
注意在使用更快的类之后，由于始终有线程在保持连接，所以其他连接只能用这个实时连接的每个线程周期的空余时间做点事，在读写寄存器的时候还是会有点问题
*/

//
namespace ModbusCommunication
{
    //定义功能码
    public enum FunctionCode : byte
    {
        //Read Multiple Registers
        Read = 3,
        //Write Multiple Registers
        Write = 16
    }

    //网口的ModbusSocket类
    class ModbusSocket
    {
        #region 静态属性
        //有静态属性（远程IP，远程端口，超时时间，起始地址）
        public string RemoteIP;
        public int RemotePort;
        public int SocketTimeOut;

        //有一个Socket对象和IPE对象
        public Socket socket = null;
        public IPEndPoint ipe;

        //我个人定义，如果写多个寄存器，则不同寄存器的值以“|”分开(不喜欢的可以自己更改)
        public char RegisterDivision = '|';

        public Thread KeepConnectThread;

        # endregion

        #region 传统的方法：多了一些try catch语句，执行效率降低了不少，但是可以主动更新数据
        //连接的方法(这是传统的连接方法)
        public void connectServer()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //设置这个socket的超时时间
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, SocketTimeOut);
            //用同步方法进行连接
            ipe = new IPEndPoint(IPAddress.Parse(RemoteIP), RemotePort);
            try
            {
                socket.Connect(ipe);
            }
            catch
            {
                MessageBox.Show("未取得UR的连接，请确认连接正常。");
            }
        }

        //主动关闭Socket连接的方法(这是传统的连接方法)
        public void closeServer()
        {
            bool closeMyself = true;
            if (closeMyself)
            {
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                catch{}
            } 
        }

        #endregion

        #region 更快的方法：尽可能少用try catch，但是无法主动更新Server的数据，尤其是寄存器写入，经常会有的写不进去
        //初始化的方法（这是实时性要求较高的方法，初始化一次，以后不再new这么多类的实例，直接保持Connect）
        public void initialServer()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //设置这个socket的超时时间
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, SocketTimeOut);
            //用同步方法进行连接
            ipe = new IPEndPoint(IPAddress.Parse(RemoteIP), RemotePort);

            //socket.LingerState.LingerTime = 100000000;

            //同步的方法，只在socket实例化的时候连接一次
            socket.Connect(ipe);

            KeepConnectThread = new Thread(keepConnectServer);
            KeepConnectThread.Start();
        }

        //保持连接的方法，因为如果client向Server发送请求的速度太慢，则往往会被自动中断这个连接
        public void keepConnectServer()
        {
            //这是定期在做的，不需要干预
            Thread.Sleep(1000);
            if (!socket.Connected)
            {
                this.initialServer();
            }
        }

        #endregion

        //WriteMultipleRegister方法需要传入写入寄存器的字符串（如“123|234|5555”，如果只有一个则不用分隔符），要写入寄存器的个数（1,2,3）要一一对应和写入的起始地址
        public void WriteMultipleRegister(string RegisterString,int RegisterNum,int StartAddress,bool IfSlow)
        {
            //将传入的字符串按RegisterDivision进行分割
            string[] RegisterStringArray = RegisterString.Split(new char[] { RegisterDivision });

            //把分割好的字符串存入byte数组(这里已经知道了到底要写入几个寄存器，也就知道了数组长度，因为寄存器分高低，如果我只要写1个寄存器，则就要有2个byte，以此类推)
            byte[] data = new byte[RegisterNum*2];

            //如果发现要写入的寄存器数量与给的不一致，则提示
            if (RegisterStringArray.Length != RegisterNum)
            {
                MessageBox.Show("你到底要写入几个寄存器，方法参数对不上");
            }
            else
            { 
                 //把分割好的每个寄存器要写入的值依次添加进来
                for (int i = 0; i < RegisterStringArray.Length; i++)
                {
                    //把每个数字提取到tempArray里面
                    byte[] tempArray = IntTobyteArray(Convert.ToInt32(RegisterStringArray[i]));
                    data[i*2] = tempArray[0];
                    data[i*2+1] = tempArray[1];
                }

                //如果要求慢速的话，则使用传统的连接方法(用到了try catch)
                if (IfSlow)
                {
                    try
                    {
                        this.connectServer();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("未取得UR的连接，请确认连接正常。");
                    }
                }

                List<byte> values = new List<byte>(255);

                //1 凑Header

                //1.1（数据位：1-2） 定义我是谁(Transaction Identifier),协议给了我2byte空间定义我是谁（我代号就是01，如果Modbus TCP Server接收成功，返回信息也要有这个代号）
                values.AddRange(new Byte[] { 0, 1 });

                //1.2 （数据位：3-4）定义协议号，协议给了我2byte空间定义协议号（协议号就是00，表示这是MODBUS 协议）
                values.AddRange(new Byte[] { 0, 0 });

                //1.3 （数据位：5-6）现在考虑将56两位凑到一块
                byte[] DataByte = BitConverter.GetBytes((data.Length + 7));
                values.Add(DataByte[1]);
                values.Add(DataByte[0]);

                //1.4 （数据位：7）只要补全一个0即可
                values.Add(0);

                //1.5 （数据位：8）定义功能码（把写多个寄存器这个16码转换为byte类型数据）Function Code : 16 (Write Multiple Register)
                values.Add((byte)FunctionCode.Write);

                //1.6 （数据位：9-10）起始地址，现在把地址这个int类型转换为两个byte(地址他说我差1，我就给他减掉1好了)
                byte[] AddressByte = BitConverter.GetBytes(StartAddress);
                values.Add(AddressByte[1]);
                values.Add(AddressByte[0]);

                //1.7  （数据位：11-12）寄存器数量（不可能超过255个）
                values.Add(0);
                values.Add((byte)RegisterNum);

                //1.8 （数据位：13）发送数据的长度，跟前面保持不变
                values.Add((byte)data.Length);

                //2 发数据
                values.AddRange(data);
                socket.Send(values.ToArray());

            }

        }

        //ReadMultipleRegister需要传入的参数包括：要读取的寄存器个数（比如1,2,3），要读取的寄存器首地址（比如100）
        public int[] ReadMultipleRegister(int ResigterNum,int StartAddress,bool IfSlow)
        {
            //我确实要读取的东西(byte永远是两个表示一个)
            byte[] TruelyDateByte = new byte[ResigterNum*2];
            int[] TruelyDateInt = new int[ResigterNum];

            //如果要求慢速的话，则使用传统的连接方法(用到了try catch)
            if (IfSlow)
            {
                try
                {
                    this.connectServer();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            try
            {
            //定义长度不确定的数组sendData，每个数组元素都是byte类型的整数
            List<byte> sendData = new List<byte>(255);

            //1 凑Header

            //1.1（数据位：1-2） 定义我是谁(Transaction Identifier),协议给了我2byte空间定义我是谁（我代号就是01，如果Modbus TCP Server接收成功，返回信息也要有这个代号）
            sendData.AddRange(new Byte[] { 0, 1 });

            //1.2 （数据位：3-4）定义协议号，协议给了我2byte空间定义协议号（协议号就是00，表示这是MODBUS 协议）
            sendData.AddRange(new Byte[] { 0, 0 });

            //1.3 （数据位：5-6）header还有几位？（对于读取来说，header后面定死了还有6位）
            byte[] DataByte = BitConverter.GetBytes(6);
            sendData.Add(DataByte[1]);
            sendData.Add(DataByte[0]);

            //1.4 （数据位：7）只要补全一个0即可
            sendData.Add(0);

            //1.5 （数据位：8）定义功能码（把读多个寄存器这个03码转换为byte类型数据）Function Code : 03 (Read Multiple Registers)
            sendData.Add((byte)FunctionCode.Read);

            //1.6 （数据位：9-10）起始地址，现在把地址这个int类型转换为两个byte
            byte[] AddressByte = BitConverter.GetBytes(StartAddress);
            sendData.Add(AddressByte[1]);
            sendData.Add(AddressByte[0]);

            //1.7  （数据位：11-12）寄存器数量，默认要读取的寄存器数量不超过255个，所以高位不管了
            sendData.Add(0);
            sendData.Add((byte)ResigterNum);
            if (socket.Connected == false)
                return TruelyDateInt;
            //2 发送查询命令
            socket.Send(sendData.ToArray());//

            //立即等待返回（由于是同步的操作，所以定义“我是谁”毫无意义，除非是异步的，返回值不知道要返回给谁）
            int ReadBuffer = 256;
            byte[] ReadBufferData = new byte[ReadBuffer];
            
            socket.Receive(ReadBufferData);
            //socket的Receive方法直接把读取到的数据返回给ReadBufferData
            //虽然一次把所有都读出来，但是并不是所有都是我需要的，我前面规定了我要读取几个，你给我返回几个就可以了
            //（0|1|0|0|0|9|0|3|6|）前面9位对我来说都是无意义的
            for (int i = 0; i < TruelyDateByte.Length; i++)
            {
                TruelyDateByte[i] = ReadBufferData[i + 9];
            }

            //然后byte再放回int
            for (int i = 0; i < TruelyDateInt.Length;i++ )
            {
                TruelyDateInt[i] = TruelyDateByte[i * 2] * 256 + TruelyDateByte[i * 2+1];
            }
            
            }
            catch
            {

            }
            //如果要求慢速，则关闭自己(用到了try catch)
            if (IfSlow)
            {
                try
                {
                    this.closeServer();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            return TruelyDateInt;

        }

        //定义一个将int类型数据转换为寄存器高低字节的方法（int大小为0-65535）
        public byte[] IntTobyteArray(int InputNum)
        {
            byte[] temp = new byte[2];
            int InputNum_Hign = InputNum / 256;
            int InputNum_Low = InputNum % 256;

            //整数部分给高位，余数部分给低位
            temp[0] = (byte)InputNum_Hign;
            temp[1] = (byte)InputNum_Low;

            return temp;
        }


    }
}
