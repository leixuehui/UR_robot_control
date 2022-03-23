using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


//调用外部类
using Files;
using URDate;
using URControl;

namespace UR_点动控制器
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Form.CheckForIllegalCrossThreadCalls = false;
        }


        //主程序不应该关心细枝末节，只要知道问谁要到数据，还有要把数据给谁
        URDateHandle URDateCollector = new URDateHandle();
        URControlHandle URController = new URControlHandle();

        //声明全局的速度和加速度控制条
        public double SpeedRate;
        public double AccelerationRate;

        //这五个参数做成全局的会比较好用
        public double BasicSpeed;
        public double BasicAcceleration;
        public string Target_IP;
        public int Control_Port;
        public int DataRefreshRate;

        public double[] PreviousAngles = new double[6];
        public bool CurrentRunningState = false;

        //把所需要的图像也在初始化的时候就全部得到
        public Bitmap RobotState_Poweroff;
        public Bitmap RobotState_Ready;
        public Bitmap RobotState_SecurityStopped;
        public Bitmap RobotState_EmergencyStoped;
        public Bitmap RobotState_Teaching;

        public Bitmap RobotJog_XYZLeftClick;
        public Bitmap RobotJog_XYZLeftNormal;
        public Bitmap RobotJog_XYZRightClick;
        public Bitmap RobotJog_XYZRightNormal;
        public Bitmap RobotJog_XYZBackClick;
        public Bitmap RobotJog_XYZBackNormal;
        public Bitmap RobotJog_XYZForwardClick;
        public Bitmap RobotJog_XYZForwardNormal;
        public Bitmap RobotJog_XYZUpClick;
        public Bitmap RobotJog_XYZUpNormal;
        public Bitmap RobotJog_XYZDownClick;
        public Bitmap RobotJog_XYZDownNormal;
        public Bitmap RobotJog_RotateLeftClick;
        public Bitmap RobotJog_RotateLeftNormal;
        public Bitmap RobotJog_RotateRightClick;
        public Bitmap RobotJog_RotateRightNormal;

        //声明默认的配置文件路径
        public string DefaultINIPath = Convert.ToString(System.AppDomain.CurrentDomain.BaseDirectory)  + "Config.ini";

        private void Form1_Load(object sender, EventArgs e)
        {
            
            //执行委托的绑定
            URDateCollector.OnGetPositionSuccess += new URDateHandle.GetPositionSuccess(UpdatePositionsValue);
            URDateCollector.OnGetAngleSuccess += new URDateHandle.GetAngleSuccess(UpdateAnglesValue);
            URDateCollector.OnGetRobotStateSuccess += new URDateHandle.GetRobotStateSuccess(UpdateRobotState);
            URDateCollector.OnGetRobotSpeedSuccess += new URDateHandle.GetRobotSpeedSuccess(UpdateRobotSpeed);

            //这里直接读取配置文件是否启用了自动连接
            FilesINI ConfigController = new FilesINI();
            string AutoConnection = ConfigController.INIRead("UR控制参数", "IfAutoConnect", DefaultINIPath);


            //这里读取到每个图片，方便下面修状态信息
            Do_Initilize_Image();

            //这里初始化速度值，其实速度值一直在跳变的
            Do_Initilize_RobotSpeed();

            //如果启用了自动连接，则直接获取所有自动连接参数，并运行连接方法
            if (AutoConnection == "YES")
            {
                //这里初始化的是获取机器人参数
                Do_Initilize();
            }


        }

        //不管用户是否勾选自动连接，手动连接都是执行这个方法，区别只是改好了配置文件再连接还是不用改就可以连接
        private void Do_Initilize()
        {
            FilesINI ConfigController = new FilesINI();

            Target_IP = ConfigController.INIRead("UR控制参数", "RemoteIP", DefaultINIPath);
            Control_Port = Convert.ToInt32(ConfigController.INIRead("UR控制参数", "RemoteControlPort", DefaultINIPath));
            DataRefreshRate = Convert.ToInt32(ConfigController.INIRead("UR运动参数", "BasicRefreshRate", DefaultINIPath));

            BasicSpeed = Convert.ToDouble(ConfigController.INIRead("UR运动参数", "BasicSpeed", DefaultINIPath));
            BasicAcceleration = Convert.ToDouble(ConfigController.INIRead("UR运动参数", "BasicAcceleration", DefaultINIPath));


            //我在URDateHandle中定义了刷新速度是静态的，所以可以直接赋值(先赋值，后实例化对象，否则直接运行就报错)
            URDateHandle.ScanRate = DataRefreshRate;

            //初始化URDateCollector，开始从502端口实时采集UR数据(需要提供要采集UR的IP地址)
            URDateCollector.InitialMoniter(Target_IP);

            //初始化URControlHandle，生成一个clientSocket，方便从30001-30003端口直接发送指令
            URController.Creat_client(Target_IP, Control_Port);

            //初始化速度和加速度(基准速度0.15 最高变成2倍即0.2，最低变成0.1倍即0.01)
            SpeedRate = BasicSpeed * SpeedBar.Value / 10;
            AccelerationRate = BasicAcceleration * AccelerationBar.Value / 10;
        }

        //把主界面所需要的图片都从文件中实例化
        private void Do_Initilize_Image()
        {
            string DebugDir = Convert.ToString(System.AppDomain.CurrentDomain.BaseDirectory);

            #region 机器人状态的图片
            string RobotState_Poweroff_PicDir = DebugDir + "\\Button\\BtnPoweroff.png";
            RobotState_Poweroff = new Bitmap(RobotState_Poweroff_PicDir);

            string RobotState_Ready_PicDir = DebugDir + "\\Button\\BtnReady.png";
            RobotState_Ready = new Bitmap(RobotState_Ready_PicDir);

            string RobotState_SecurityStopped_PicDir = DebugDir + "\\Button\\BtnSecurityStopped.png";
            RobotState_SecurityStopped = new Bitmap(RobotState_SecurityStopped_PicDir);

            string RobotState_EmergencyStoped_PicDir = DebugDir + "\\Button\\BtnEmergencyStoped.png";
            RobotState_EmergencyStoped = new Bitmap(RobotState_EmergencyStoped_PicDir);

            string RobotState_Teaching_PicDir = DebugDir + "\\Button\\BtnTeaching.png";
            RobotState_Teaching = new Bitmap(RobotState_Teaching_PicDir);

            #endregion

            #region XYZ控制按钮
            //XYZ左移按钮
            string RobotJog_XYZLeftClick_PicDir = DebugDir + "\\Arrow\\ArrowXYZLeft_click.png";
            RobotJog_XYZLeftClick = new Bitmap(RobotJog_XYZLeftClick_PicDir);

            string RobotJog_XYZLeftNormal_PicDir = DebugDir + "\\Arrow\\ArrowXYZLeft_normal.png";
            RobotJog_XYZLeftNormal = new Bitmap(RobotJog_XYZLeftNormal_PicDir);

            //XYZ右移按钮
            string RobotJog_XYZRightClick_PicDir = DebugDir + "\\Arrow\\ArrowXYZRight_click.png";
            RobotJog_XYZRightClick = new Bitmap(RobotJog_XYZRightClick_PicDir);

            string RobotJog_XYZRightNormal_PicDir = DebugDir + "\\Arrow\\ArrowXYZRight_normal.png";
            RobotJog_XYZRightNormal = new Bitmap(RobotJog_XYZRightNormal_PicDir);

            //XYZ后移按钮
            string RobotJog_XYZBackClick_PicDir = DebugDir + "\\Arrow\\ArrowXYZBack_click.png";
            RobotJog_XYZBackClick = new Bitmap(RobotJog_XYZBackClick_PicDir);

            string RobotJog_XYZBackNormal_PicDir = DebugDir + "\\Arrow\\ArrowXYZBack_normal.png";
            RobotJog_XYZBackNormal = new Bitmap(RobotJog_XYZBackNormal_PicDir);

            //XYZ前移按钮
            string RobotJog_XYZForwardClick_PicDir = DebugDir + "\\Arrow\\ArrowXYZForward_click.png";
            RobotJog_XYZForwardClick = new Bitmap(RobotJog_XYZForwardClick_PicDir);

            string RobotJog_XYZForwardNormal_PicDir = DebugDir + "\\Arrow\\ArrowXYZForward_normal.png";
            RobotJog_XYZForwardNormal = new Bitmap(RobotJog_XYZForwardNormal_PicDir);

            //XYZ上移按钮
            string RobotJog_XYZUpClick_PicDir = DebugDir + "\\Arrow\\ArrowXYZUp_click.png";
            RobotJog_XYZUpClick = new Bitmap(RobotJog_XYZUpClick_PicDir);

            string RobotJog_XYZUpNormal_PicDir = DebugDir + "\\Arrow\\ArrowXYZUp_normal.png";
            RobotJog_XYZUpNormal = new Bitmap(RobotJog_XYZUpNormal_PicDir);

            //XYZ下移按钮
            string RobotJog_XYZDownClick_PicDir = DebugDir + "\\Arrow\\ArrowXYZDown_click.png";
            RobotJog_XYZDownClick = new Bitmap(RobotJog_XYZDownClick_PicDir);

            string RobotJog_XYZDownNormal_PicDir = DebugDir + "\\Arrow\\ArrowXYZDown_normal.png";
            RobotJog_XYZDownNormal = new Bitmap(RobotJog_XYZDownNormal_PicDir);

            #endregion


            #region 六轴运动按钮
            //六轴旋转的左移按钮
            string RobotJog_RotateLeftClick_PicDir = DebugDir + "\\Arrow\\Arrow_left_click.png";
            RobotJog_RotateLeftClick = new Bitmap(RobotJog_RotateLeftClick_PicDir);

            string RobotJog_RotateLeftNormal_PicDir = DebugDir + "\\Arrow\\Arrow_left_normal.png";
            RobotJog_RotateLeftNormal = new Bitmap(RobotJog_RotateLeftNormal_PicDir);

            //六轴旋转的右移按钮
            string RobotJog_RotateRightClick_PicDir = DebugDir + "\\Arrow\\Arrow_right_click.png";
            RobotJog_RotateRightClick = new Bitmap(RobotJog_RotateRightClick_PicDir);

            string RobotJog_RotateRightNormal_PicDir = DebugDir + "\\Arrow\\Arrow_right_normal.png";
            RobotJog_RotateRightNormal = new Bitmap(RobotJog_RotateRightNormal_PicDir);

            #endregion

            //默认初始化的时候是这幅图片
            RobotStatusPic.Image = RobotState_Poweroff;

        }

        private void Do_Initilize_RobotSpeed()
        {
            //如果要检测UR的TCP速度，但是由于晃动问题，请使用get_target_tcp_speed方法
            label_VX.Visible = false;
            label_VY.Visible = false;
            label_VZ.Visible = false;
            label_VRX.Visible = false;
            label_VRY.Visible = false;
            label_VRZ.Visible = false;
        
        }

        private void SpeedChange(object sender, EventArgs e)
        {
            SpeedRate = BasicSpeed * SpeedBar.Value / 10;
        }

        private void AccelerationChange(object sender, EventArgs e)
        {
            AccelerationRate = BasicAcceleration * AccelerationBar.Value / 10;
        }

        //退出程序要把所有都释放掉
        private void QuitApp(object sender, FormClosingEventArgs e)
        {
            URController.Close_client();
            URController = null;
        }

        //将取到的数据放入文本框(当需要被通知时候触发)
        void UpdatePositionsValue(float[] Positions)
        { 
		try{
            if(URController != null)
            {
                X_Position.Text = Positions[0].ToString("0.0");
                Y_Position.Text = Positions[1].ToString("0.0");
                Z_Position.Text = Positions[2].ToString("0.0");
                U_Position.Text = Positions[3].ToString("0.000");
                V_Position.Text = Positions[4].ToString("0.000");
                W_Position.Text = Positions[5].ToString("0.000");

            }
			}
			catch{}
        }

        void UpdateAnglesValue(double[] Angles)
        {
            try
            {
                int[] AngleBar_Values = new int[6];

                //由于Angle已经取到的是正负360度，所以正负要做区分
                for (int i = 0; i < Angles.Length; i++)
                {
                    if (Angles[i] < 0)
                    {
                        AngleBar_Values[i] = 360 - Math.Abs(Convert.ToInt32(Angles[i]));
                    }
                    else
                    {
                        AngleBar_Values[i] = 360 + Math.Abs(Convert.ToInt32(Angles[i]));
                    }
                }

                //这里使用了自定义控件，所以不再是Value属性
                AngleBarX.Position = AngleBar_Values[0];
                AngleBarY.Position = AngleBar_Values[1];
                AngleBarZ.Position = AngleBar_Values[2];
                AngleBarU.Position = AngleBar_Values[3];
                AngleBarV.Position = AngleBar_Values[4];
                AngleBarW.Position = AngleBar_Values[5];

                AngleBarX.Text = Angles[0].ToString("0.00") + "  °";
                AngleBarY.Text = Angles[1].ToString("0.00") + "  °";
                AngleBarZ.Text = Angles[2].ToString("0.00") + "  °";
                AngleBarU.Text = Angles[3].ToString("0.00") + "  °";
                AngleBarV.Text = Angles[4].ToString("0.00") + "  °";
                AngleBarW.Text = Angles[5].ToString("0.00") + "  °";

                int TotalCount = 0;
                //这里除了把六个角度值都放到界面中显示，还根据六个角度值的变化情况，得出机器人是否在运行的结果（大致的判断方法）
                for (int i = 0; i < 6; i++)
                {
                    //只要不等，就让他相等(暂时认为是两者的差值在我指定的误差范围之内，而不是绝对相等)
                    if (Math.Abs(PreviousAngles[i] - Angles[i]) > 0.02)
                    {
                        //只要发现一个超过范围的，就让总体计数值加1，看最后有多少不一样
                        TotalCount++;
                    }
                    PreviousAngles[i] = Angles[i];
                }

                //如果最多只有1个数据变动较小，则不管，认为机器人没动，否则认为动了
                if (TotalCount > 2)
                {
                    CurrentRunningState = true;
                }
                else
                {
                    CurrentRunningState = false;
                }

                RobotRunningLabel.Text = CurrentRunningState.ToString();//System.InvalidOperationException: '集合在枚举数实例化后进行了修改
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        void UpdateRobotState(int[] RobotState)
        {
            if (RobotState[1] == 1)
            {
                try
                {
                    RobotStatusPic.Image = RobotState_SecurityStopped;
                    this.RobotStatusLabel.Text = "安全停机";
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            if (RobotState[2] == 1)
            {
                try
                {
                    RobotStatusPic.Image = RobotState_EmergencyStoped;
                    this.RobotStatusLabel.Text = "紧急停机";
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            if (RobotState[3] == 1)
            {
                try
                {
                    RobotStatusPic.Image = RobotState_Teaching;
                    this.RobotStatusLabel.Text = "示教模式";
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            /*
            //这里的判断一定要跟前面区分开来，只有前面都不满足的时候这里才判断，否则这里的结果会和前面冲突，显示到界面上就会闪烁了
            if (RobotState[1] != 1 && RobotState[2] != 1 && RobotState[3] != 1)
            {
                if (RobotState[0] ==1)
                {
                    try
                    {
                        RobotStatusPic.Image = RobotState_Ready;
                        this.RobotStatusLabel.Text = "已连接";
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }

            }*/
        }

        void UpdateRobotSpeed(int[] RobotSpeed)
        {
            //更新速度信息有一个很严重的问题，由于UR的抖动的不确定性，即如果UR没有运行，但是末端还是会有微量的移动，所以速度会一直在跳变
            //如果此时按下示教按钮再松开，则抖动问题就没了，再按下示教按钮，再松开，问题又出现了，在我们无法确切的排除掉抖动问题之前，我们无法靠TCP的速度的寄存器来判定当前机器人是否在运行
            /*
            try
            {
                label_VX.Text = RobotSpeed[0].ToString("0.0000");
                label_VY.Text = RobotSpeed[1].ToString("0.0000");
                label_VZ.Text = RobotSpeed[2].ToString("0.0000");
                label_VRX.Text = RobotSpeed[3].ToString("0.0000");
                label_VRY.Text = RobotSpeed[4].ToString("0.0000");
                label_VRZ.Text = RobotSpeed[5].ToString("0.0000");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
*/
        }


        //对于XYZ的线性移动，需要定义一个方法，只需要传入要移动的轴和移动方向(方向就是1和-1)，返回移动的命令
        string GetLinearMovementCommand(string whatAxis,int direction)
        { 
            //不管怎么样都要获取当前的坐标值
            double new_X = URDateHandle.Positions_X;
            double new_Y = URDateHandle.Positions_Y;
            double new_Z = URDateHandle.Positions_Z;
            double new_U = URDateHandle.Positions_U;
            double new_V = URDateHandle.Positions_V;
            double new_W = URDateHandle.Positions_W;

            //然后根据点动的按钮，判断要改哪个值(这里不是旋转，只有X,Y,Z三种可能)，直接覆盖到真实的当前XYZ值
            if (whatAxis == "X")
            {
                new_X = ((new_X + 10) * direction);
            }
            else if (whatAxis == "Y")
            {
                new_Y = ((new_Y + 10) * direction);
            }
            else if (whatAxis == "Z")
            {
                new_Z = ((new_Z + 10) * direction);
            }
            else
            { 
                //也有可能我不要移动，只是要看指令
            }


            //最后把方向运动的指令发送出去
            string command = "movel(p[" + new_X.ToString("0.0000") + "," + new_Y.ToString("0.0000") + "," + new_Z.ToString("0.0000") + "," + new_U.ToString("0.0000") + "," + new_V.ToString("0.0000") + "," + new_W.ToString("0.0000") + "], a = " + AccelerationRate.ToString() + ", v = " + SpeedRate.ToString() + ")";
            CustomCommand.Text = command;
            return command;
        }

        //对于六轴转动，跟前面类似
        string GetRotationMovementCommand(string whatAxis,int direction)
        {
            //不管怎么样都要获取当前的六个关节值
            double new_X = URDateHandle.Angles_X;
            double new_Y = URDateHandle.Angles_Y;
            double new_Z = URDateHandle.Angles_Z;
            double new_U = URDateHandle.Angles_U;
            double new_V = URDateHandle.Angles_V;
            double new_W = URDateHandle.Angles_W;

            if (whatAxis == "X")
            {
                new_X = ((new_X + 100) * direction);
            }
            else if (whatAxis == "Y")
            {
                new_Y = ((new_Y + 100) * direction);
            }
            else if (whatAxis == "Z")
            {
                new_Z = ((new_Z + 100) * direction);
            }
            else if (whatAxis == "U")
            {
                new_U = ((new_U + 100) * direction);
            }
            else if (whatAxis == "V")
            {
                new_V = ((new_V + 100) * direction);
            }
            else if (whatAxis == "W")
            {
                new_W = ((new_W + 100) * direction);
            }
            else
            {
                //也有可能我不要移动，只是要看指令
            }
            //最后把方向运动的指令发送出去
            string command = "movej([" + new_X.ToString("0.0000") + "," + new_Y.ToString("0.0000") + "," + new_Z.ToString("0.0000") + "," + new_U.ToString("0.0000") + "," + new_V.ToString("0.0000") + "," + new_W.ToString("0.0000") + "], a = " + AccelerationRate.ToString() + ", v = " + SpeedRate.ToString() + ")";
            CustomCommand.Text = command;
            return command;

        }

        //发送停止命令则很简单了，都是发送stopl(1)
        string GetStopCommand()
        {
            string StopCommand = "stopl(1)";
            CustomCommand.Text = StopCommand;
            return StopCommand;
        }

        # region XYZ平移区域
        //XYZ左移按钮按下
        private void Move_Left_Down(object sender, MouseEventArgs e)
        {
            try
            {
                this.Move_Left.Image = RobotJog_XYZLeftClick;
                string str = GetLinearMovementCommand("X", 1);
                URController.Send_command(str);
            }
            catch { }

        }
        //XYZ左移按钮松开
        private void Move_Left_Up(object sender, MouseEventArgs e)
        {
            try
            {
                this.Move_Left.Image = RobotJog_XYZLeftNormal;
                string str = GetStopCommand();
                URController.Send_command(str);
            }
            catch { }

        }
        //XYZ右移按钮按下
        private void Move_Right_Down(object sender, MouseEventArgs e)
        {
            try
            {
                this.Move_Right.Image = RobotJog_XYZRightClick;
                string str = GetLinearMovementCommand("X", -1);
                URController.Send_command(str);
            }
            catch { }

        }
        //XYZ右移按钮松开
        private void Move_Right_Up(object sender, MouseEventArgs e)
        {
            try
            {
                this.Move_Right.Image = RobotJog_XYZRightNormal;
                string str = GetStopCommand();
                URController.Send_command(str);
            }
            catch { }

        }
        //XYZ后移按钮按下
        private void Move_Back_Down(object sender, MouseEventArgs e)
        {
            try
            {
                this.Move_Back.Image = RobotJog_XYZBackClick;
                string str = GetLinearMovementCommand("Y", -1);
                URController.Send_command(str);
            }
            catch { }

        }
        //XYZ后移按钮松开
        private void Move_Back_Up(object sender, MouseEventArgs e)
        {
            try
            {
                this.Move_Back.Image = RobotJog_XYZBackNormal;
                string str = GetStopCommand();
                URController.Send_command(str);
            }
            catch { }

        }
        //XYZ前移按钮按下
        private void Move_Forward_Down(object sender, MouseEventArgs e)
        {
            try
            {
                this.Move_Forward.Image = RobotJog_XYZForwardClick;
                string str = GetLinearMovementCommand("Y", 1);
                URController.Send_command(str);
            }
            catch { }
        }
        //XYZ前移按钮松开
        private void Move_Forward_Up(object sender, MouseEventArgs e)
        {
            try
            {
                this.Move_Forward.Image = RobotJog_XYZForwardNormal;
                string str = GetStopCommand();
                URController.Send_command(str);
            }
            catch { }
        }
        //XYZ上移按钮按下
        private void Move_Up_Down(object sender, MouseEventArgs e)
        {
            try
            {
                this.Move_Up.Image = RobotJog_XYZUpClick;
                string str = GetLinearMovementCommand("Z", 1);
                URController.Send_command(str);
            }
            catch { }
        }
        //XYZ上移按钮松开
        private void Move_Up_Up(object sender, MouseEventArgs e)
        {
            try
            {
                this.Move_Up.Image = RobotJog_XYZUpNormal;
                string str = GetStopCommand();
                URController.Send_command(str);
            }
            catch { }
        }
        //XYZ下移按钮按下
        private void Move_Down_Down(object sender, MouseEventArgs e)
        {
            try
            {
                this.Move_Down.Image = RobotJog_XYZDownClick;
                string str = GetLinearMovementCommand("Z", -1);
                URController.Send_command(str);
            }
            catch { }
        }
        //XYZ下移按钮松开
        private void Move_Down_Up(object sender, MouseEventArgs e)
        {
            try
            {
                this.Move_Down.Image = RobotJog_XYZDownNormal;
                string str = GetStopCommand();
                URController.Send_command(str);
            }
            catch { }
        }

        #endregion

        #region X左右旋转

        //六轴旋转（X向左转按下）
        private void X_Left_Rotate_Down(object sender, MouseEventArgs e)
        {
            try
            {
                this.X_Left_Rotate.Image = RobotJog_RotateLeftClick;
                string str = GetRotationMovementCommand("X", -1);
                URController.Send_command(str);
            }
            catch { }


        }
        //六轴旋转（X向左转松开）
        private void X_Left_Rotate_Up(object sender, MouseEventArgs e)
        {
            try
            {
                this.X_Left_Rotate.Image = RobotJog_RotateLeftNormal;
                string str = GetStopCommand();
                URController.Send_command(str);
            }
            catch { }
        }
        //六轴旋转（X向右转按下）
        private void X_Right_Rotate_Down(object sender, MouseEventArgs e)
        {
            try
            {
                this.X_Right_Rotate.Image = RobotJog_RotateRightClick;
                string str = GetRotationMovementCommand("X", 1);
                URController.Send_command(str);
            }
            catch { }
        }
        //六轴旋转（X向右转松开）
        private void X_Right_Rotate_Up(object sender, MouseEventArgs e)
        {
            try
            {
                this.X_Right_Rotate.Image = RobotJog_RotateRightNormal;
                string str = GetStopCommand();
                URController.Send_command(str);
            }
            catch { }
        }

        //六轴旋转（Y向左转按下）
        private void Y_Left_Rotate_Down(object sender, MouseEventArgs e)
        {
            try
            {
                this.Y_Left_Rotate.Image = RobotJog_RotateLeftClick;
                string str = GetRotationMovementCommand("Y", -1);
                URController.Send_command(str);
            }
            catch { }
        }
        //六轴旋转（Y向左转松开）
        private void Y_Left_Rotate_Up(object sender, MouseEventArgs e)
        {
            try
            {
                this.Y_Left_Rotate.Image = RobotJog_RotateLeftNormal;
                string str = GetStopCommand();
                URController.Send_command(str);
            }
            catch { }
        }

        //六轴旋转（Y向右转按下）
        private void Y_Right_Rotate_Down(object sender, MouseEventArgs e)
        {
            try
            {
                this.Y_Right_Rotate.Image = RobotJog_RotateRightClick;
                string str = GetRotationMovementCommand("Y", 1);
                URController.Send_command(str);
            }
            catch { }
        }
        //六轴旋转（Y向右转松开）
        private void Y_Right_Rotate_Up(object sender, MouseEventArgs e)
        {
            try
            {
                this.Y_Right_Rotate.Image = RobotJog_RotateRightNormal;
                string str = GetStopCommand();
                URController.Send_command(str);
            }
            catch { }
        }


        //六轴旋转（Z向左转按下）
        private void Z_Left_Rotate_Down(object sender, MouseEventArgs e)
        {
            try
            {
                this.Z_Left_Rotate.Image = RobotJog_RotateLeftClick;
                string str = GetRotationMovementCommand("Z", -1);
                URController.Send_command(str);
            }
            catch { }
        }
        //六轴旋转（Z向左转松开）
        private void Z_Left_Rotate_Up(object sender, MouseEventArgs e)
        {
            try
            {
                this.Z_Left_Rotate.Image = RobotJog_RotateLeftNormal;
                string str = GetStopCommand();
                URController.Send_command(str);
            }
            catch { }
        }
        //六轴旋转（Z向右转按下）
        private void Z_Right_Rotate_Down(object sender, MouseEventArgs e)
        {
            try
            {
                this.Z_Right_Rotate.Image = RobotJog_RotateRightClick;
                string str = GetRotationMovementCommand("Z", 1);
                URController.Send_command(str);
            }
            catch { }
        }
        //六轴旋转（Z向右转松开）
        private void Z_Right_Rotate_Up(object sender, MouseEventArgs e)
        {
            try
            {
                this.Z_Right_Rotate.Image = RobotJog_RotateRightNormal;
                string str = GetStopCommand();
                URController.Send_command(str);
            }
            catch { }
        }

        //六轴旋转（U向左转按下）
        private void U_Left_Rotate_Down(object sender, MouseEventArgs e)
        {
            try
            {
                this.U_Left_Rotate.Image = RobotJog_RotateLeftClick;
                string str = GetRotationMovementCommand("U", -1);
                URController.Send_command(str);
            }
            catch { }
        }
        //六轴旋转（U向左转松开）
        private void U_Left_Rotate_Up(object sender, MouseEventArgs e)
        {
            try
            {
                this.U_Left_Rotate.Image = RobotJog_RotateLeftNormal;
                string str = GetStopCommand();
                URController.Send_command(str);
            }
            catch { }
        }
        //六轴旋转（U向右转按下）
        private void U_Right_Rotate_Down(object sender, MouseEventArgs e)
        {
            try
            {
                this.U_Right_Rotate.Image = RobotJog_RotateRightClick;
                string str = GetRotationMovementCommand("U", 1);
                URController.Send_command(str);
            }
            catch { }
        }
        //六轴旋转（U向右转松开）
        private void U_Right_Rotate_Up(object sender, MouseEventArgs e)
        {
            try
            {
                this.U_Right_Rotate.Image = RobotJog_RotateRightNormal;
                string str = GetStopCommand();
                URController.Send_command(str);
            }
            catch { }
        }

        //六轴旋转（V向左转按下）
        private void V_Left_Rotate_Down(object sender, MouseEventArgs e)
        {
            try
            {
                this.V_Left_Rotate.Image = RobotJog_RotateLeftClick;
                string str = GetRotationMovementCommand("V", -1);
                URController.Send_command(str);
            }
            catch { }
        }
        //六轴旋转（V向左转松开）
        private void V_Left_Rotate_Up(object sender, MouseEventArgs e)
        {
            try
            {
                this.V_Left_Rotate.Image = RobotJog_RotateLeftNormal;
                string str = GetStopCommand();
                URController.Send_command(str);
            }
            catch { }
        }
        //六轴旋转（V向右转按下）
        private void V_Right_Rotate_Down(object sender, MouseEventArgs e)
        {
            try
            {
                this.V_Right_Rotate.Image = RobotJog_RotateRightClick;
                string str = GetRotationMovementCommand("V", 1);
                URController.Send_command(str);
            }
            catch { }
        }
        //六轴旋转（V向右转松开）
        private void V_Right_Rotate_Up(object sender, MouseEventArgs e)
        {
            try
            {
                this.V_Right_Rotate.Image = RobotJog_RotateRightNormal;
                string str = GetStopCommand();
                URController.Send_command(str);
            }
            catch { }
        }
        //六轴旋转（W向左转按下）
        private void W_Left_Rotate_Down(object sender, MouseEventArgs e)
        {
            try
            {
                this.W_Left_Rotate.Image = RobotJog_RotateLeftClick;
                string str = GetRotationMovementCommand("W", -1);
                URController.Send_command(str);
            }
            catch { }
        }
        //六轴旋转（W向左转松开）
        private void W_Left_Rotate_Up(object sender, MouseEventArgs e)
        {
            try
            {
                this.W_Left_Rotate.Image = RobotJog_RotateLeftNormal;
                string str = GetStopCommand();
                URController.Send_command(str);
            }
            catch { }
        }
        //六轴旋转（W向右转按下）
        private void W_Right_Rotate_Down(object sender, MouseEventArgs e)
        {
            try
            {
                this.W_Right_Rotate.Image = RobotJog_RotateRightClick;
                string str = GetRotationMovementCommand("W", 1);
                URController.Send_command(str);
            }
            catch { }
        }
        //六轴旋转（W向右转松开）
        private void W_Right_Rotate_Up(object sender, MouseEventArgs e)
        {
            try
            {
                this.W_Right_Rotate.Image = RobotJog_RotateRightNormal;
                string str = GetStopCommand();
                URController.Send_command(str);
            }
            catch { }
        }
        # endregion


        #region 顶部菜单栏

        //文件-参数设置
        private void File_SetParameter_Click(object sender, EventArgs e)
        {

            //我决定还是少用一点华而不实的功能，不就是设置参数嘛，何必搞一大堆配置文件，又不是很多参数，直接打开这个窗口
            Config ConfigWindow = new Config(DefaultINIPath);
            ConfigWindow.ShowDialog();
        }
        
        //文件-手动连接
        private void File_Connect_Click(object sender, EventArgs e)
        {
            //用户没有勾选自动连接，则是每次修改好了的配置文件去读取并执行连接方法
            Do_Initilize();
        }

        //文件-断开连接
        private void File_Disconnect_Click(object sender, EventArgs e)
        {
            //用户点击断开连接，则
            URDateCollector = null;
            URController.Close_client();
        }


        //帮助-所有版本
        private void Help_AllVersion_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://pan.baidu.com/s/1i3KBSDf");
        }

        //帮助-关于本软件
        private void Help_Hint_Click(object sender, EventArgs e)
        {
            //生成一个实例
            AboutMe About = new AboutMe();
            About.ShowDialog();
        }

        //帮助-问题反馈
        private void Help_Feedback_Click(object sender, EventArgs e)
        {
            //生成一个实例
            Feedback Feed = new Feedback();
            Feed.ShowDialog();
        }

        

        //常用工具-IP修改
        private void Tools_IPChange_Click(object sender, EventArgs e)
        {
            //生成一个实例
            IPChange IPWindow = new IPChange();
            IPWindow.ShowDialog();
        }

        //常用工具-自定义命令面板(单击一次显示，再单击一次隐藏)
        private void Tools_PersonalCommand_Click(object sender, EventArgs e)
        {

            //由于点击按钮之后都会触发，所以只需要判断这三个控件的可见性即可反复的显示或隐藏
            if (CustomLabel.Visible == false)
            {
                CustomLabel.Visible = true;
                CustomCommand.Visible = true;
                btnCustomSend.Visible = true;
                Change_All_Position.Visible = true;
                Change_All_Angles.Visible = true;

                Tools_PersonalCommand.Text = "隐藏自定义命令";
            }
            else
            {
                CustomLabel.Visible = false;
                CustomCommand.Visible = false;
                btnCustomSend.Visible = false;
                Change_All_Position.Visible = false;
                Change_All_Angles.Visible = false;

                Tools_PersonalCommand.Text = "显示自定义命令";
            }
        }

        //这就是自定义命令的三个控件，只要控制他们显示与隐藏即可（btnCustomSend,CustomCommand,CustomLabel）
        private void btnCustomSend_Click(object sender, EventArgs e)
        {
            //我在测试的框子中可以放任意命令
            string str = CustomCommand.Text;
            URController.Send_command(str);
        }

        //常用工具-G代码转换面板
        private void Tools_Gcode_Click(object sender, EventArgs e)
        {
            GCode GcodeWindow = new GCode(DefaultINIPath);
            GcodeWindow.Show();
        }

        //常用工具-增强示教面板
        private void Tools_Teach_Click(object sender, EventArgs e)
        {
            Teach TeachWindow = new Teach(DefaultINIPath);
            TeachWindow.Show();
        }

        //常用工具-相机标定及特征识别面板
        private void Tools_CameraCalibrate_Click(object sender, EventArgs e)
        {
            CameraCalibration CameraWindow = new CameraCalibration(DefaultINIPath);
            CameraWindow.Show();
        }

        //常用工具-相机标定及特征追踪面板
        private void Tools_CameraTracking_Click(object sender, EventArgs e)
        {

        }

        //常用工具，相机标定及视觉分拣面板
        private void Tools_CameraSorting_Click(object sender, EventArgs e)
        {

        }


        //测试工具：寄存器读写测试
        private void Tools_RegisterTest_Click(object sender, EventArgs e)
        {
            //还是要把配置文件的地址传过去
            Register RegisterWindow = new Register(DefaultINIPath);
            RegisterWindow.Show();
        }

        //测试工具：绘图工具测试
        private void Tools_DrawingTest_Click(object sender, EventArgs e)
        {
            Painting PaintWindow = new Painting();
            PaintWindow.Show();
        }

        //测试工具：图像轮廓拟合测试
        private void Tools_ImageProfileTest_Click(object sender, EventArgs e)
        {

        }

        //测试工具：双臂协同测试面板
        private void Tools_TorsoTest_Click(object sender, EventArgs e)
        {
            Torso TorsoWindow = new Torso();
            TorsoWindow.Show();
        }


        //测试工具：Dashboard
        private void Tools_DashboardTest_Click(object sender, EventArgs e)
        {
            Dashboard DashboardWindow = new Dashboard(DefaultINIPath);
            DashboardWindow.Show();
        }



        #endregion

        //有时候我就是要往X方向走1mm，则直接修改坐标即可
        private void Change_All_Position_Click(object sender, EventArgs e)
        {
            //获取下面六个值，然后发送(并没有ABC这个轴，我只是不作处理)
            //有时候我就是需要得到当前的TCP坐标值而已
            string str = GetLinearMovementCommand("ABC", 1);
            CustomCommand.Text = str;

        }

        private void Change_All_Angles_Click(object sender, EventArgs e)
        {
            //获取下面六个值，然后发送(并没有ABC这个轴，我只是不作处理)
            //有时候我就是需要得到当前的六轴关节值而已
            string str = GetRotationMovementCommand("ABC", 1);
            CustomCommand.Text = str;
        }

        private void Help_UpdateHistory_Click(object sender, EventArgs e)
        {
            //获取当前目录(我把发布方式改成Release就是Release而不是Debug了)
            //string ReleaseDir = Convert.ToString(System.AppDomain.CurrentDomain.BaseDirectory);
            System.Diagnostics.Process.Start("Document\\history.doc");
        }

        private void HelpDocument_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("Document\\readme.doc");
        }

        private void RobotStatusPic_Click(object sender, EventArgs e)
        {
            /*
            //用一张图片来表示所有情况
            Point Point_PowerOff = new Point(760, 535);
            Point Point_Ready = new Point(680, 535);
            */
        }

        private void Test_Click(object sender, EventArgs e)
        {
            Test TestWindow = new Test(DefaultINIPath);
            TestWindow.StartPosition = FormStartPosition.CenterScreen;//使窗口居中显示
            TestWindow.Show();
        }
    }
}
