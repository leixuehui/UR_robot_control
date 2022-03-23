using System;
using System.Collections.Generic;
//using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Files;
using System.Threading;
using System.Diagnostics;
//using System.Net.Sockets;
//using System.Drawing.Imaging;
using System.Runtime.InteropServices;
//using ClosedXML;
using ClosedXML.Excel;
using OpenCvSharp;
//调用外部类
using Config;
using URDate;
using URControl;
using AppiumTest;
using AirPump;
using QRCodeDetect;
using LogManage;
using static Detect.SquareDetect;
using static Config.ConfigParam;

namespace UR_点动控制器
{
    public partial class Test : Form
    {
        public string DefaultINIPath;
        private bool URClientFlag = false;
        private bool appiumBtnFlag = false;
        private bool useAppium = true;
        private bool useAirPump = true;
        
        int count = 0;
        
        //static WriteableBitmap writeableBitmap;
        URControlHandle URController = new URControlHandle();
        Process myPro = new Process();
        //AppTest App = new AppTest();
        
        public struct RecRes//按压结果
        {
            //public string fingerID;
            public string lc;//以光斑为原点的坐标
            public string wc;//机械臂坐标
            public AppTest.retcode result;
            public string orient;
        }
        public struct RecSum//按压结果
        {
            public string fingerID;
            public int pressSum;
            public int fakeSum;
            public int passSum;
            public int failSum;
            public int lightSum;
        }
        
        public Test(string ConfigFilePath)
        {
            InitializeComponent();
            UIInit();
            Log.renameLog();
            DefaultINIPath = ConfigFilePath;
        }

        private void Test_Resize(object sender, EventArgs e)//界面最大化时使控件随之最大化
        {
            setControls(this.Width, this.Height, this);
        }
        
        //声明全局的速度和加速度控制条
        //public double SpeedRate;
        //public double AccelerationRate;
        private void UIInit()
        {
            setControls(this.Width, this.Height, this);
        }
        private void setControls(float newx, float newy, Control cons)
        {
            foreach (Control con in cons.Controls)
            {
                string mytag = con.Name;
                if (mytag.Equals("textLog"))
                {
                    con.Left = 0;
                    con.Width = (int)(newx - 15) / 2;
                    con.Height = (int)newy - 100;
                }
                if (mytag.Equals("textLogRobot"))
                {
                    con.Left = (int)(newx - 15) / 2;
                    con.Width = (int)(newx - 15) / 2;
                    con.Height = (int)newy - 100;
                }
                if (mytag.Equals("label2"))
                {
                    con.Left = (int)newx / 2;
                }
            }

        }
        
        private void Test_Load(object sender, EventArgs e)
        {
            LoadConfig();

            //MYPOINT[] pt = new MYPOINT[4];
            //pt = square_detect(null, widthInImage, heightInImage, error);
            //Adjust(pt);
            //for(int i=0;i<2;i++)
            //    DetectFakeSquare("1111");
            if (CheckExe("HRKJ_FV01") == false)//如果气泵开着则只把exeFlag置为true，不再打开
                OpenAirPumpExe(airPumpServerPath, "HRKJ_FV01.exe");
            //检查exe是否启动
            while (true)
            {
                if(CheckExe("HRKJ_FV01") == true)
                {
                    if (useAirPump)
                    {
                        Air.connect(airPumpPort);
                        Air.init();
                    }
                    break;
                }
            }
        }

        private void CreateClient(int port)
        {
            if (URClientFlag == false)
            {
                FilesINI ConfigController = new FilesINI();
                string Target_IP = ConfigController.INIRead("UR控制参数", "RemoteIP", DefaultINIPath);
                int Control_Port = Convert.ToInt32(ConfigController.INIRead("UR控制参数", "RemoteControlPort", DefaultINIPath));
                //Control_Port = 29999;
                URController.Creat_client(Target_IP, port);
                URClientFlag = true;
            }
        }
        private void CloseClient()
        {
            if (URController != null && URClientFlag == true)
            {
                URController.Close_client();
                URClientFlag = false;
            }
        }
        private void AppiumServer_Click(object sender, EventArgs e)
        {
            if (appiumBtnFlag == false)
            {
                RunCmd("taskkill /F /IM node.exe&exit");
                //AddMsgToTextBox(this.textLog, string.Format("[Appium] Appium server closed!"));
                Thread appiumThread = new Thread(new ParameterizedThreadStart(RunCmd));
                appiumThread.Start("appium");
                button3.Enabled = false;//server启动完毕后才能继续点击此按钮
                while (true)
                {
                    //Console.WriteLine("wait for server to start");
                    if (appiumBtnFlag == true)
                        break;
                }
                button3.Text = "关闭服务";
                button3.Enabled = true;
            }
            else
            {
                AppTest.QuitDriver();
                RunCmd("taskkill /F /IM node.exe&exit");
                AddMsgToTextBox(this.textLog, string.Format("[Appium] Appium server closed!"));
                button3.Text = "开启服务";
                appiumBtnFlag = false;
            }
        }
        private void Start_Test_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(testMethod);
            t.Start();
            Thread adblogThread = new Thread(new ParameterizedThreadStart(RunCmd));
            adblogThread.Start(string.Format("adb logcat>Log/{0}adblog.txt",Log.GetTime()));//同一个假手指测试时间戳用同一个
        }
        private void Pause_Test_Click(object sender, EventArgs e)
        {
            Stop();
            MessageBox.Show("测试已终止");
        }
        private void Stop()
        {
            if (useAppium)
                AppTest.QuitDriver();//appium quit
            RunCmd("taskkill /F /IM node.exe&exit");
            if (useAirPump)
                Air.close();
        }

        private List<RecRes> resList = new List<RecRes>();
        RecRes recResult = new RecRes();
        List<RecSum> sumList = new List<RecSum>();
        RecSum sumResult = new RecSum();
        [DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize")]
        public static extern int SetProcessWorkingSetSize(IntPtr process, int minSize, int maxSize);
        private static void clearMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
            }
        }
        public QRCode.QRCodeList DetectQRCode(Window window, VideoCapture capture, int sleepTime)
        {
            Mat colorImg = new Mat();
            QRCode.QRCodeList list = new QRCode.QRCodeList();
            int detectCount = 0;
            while(true)
            {
                capture.Read(colorImg);
                if (colorImg.Width == 0 || colorImg.Height == 0 || colorImg.Empty())//读视频出错时重启
                {
                    capture.Release();
                    capture = new VideoCapture(0);
                }
                if (colorImg.Width > 0 && colorImg.Height > 0)
                {
                    list = QRCode.qrcode_detect(colorImg, colorImg.Width, colorImg.Height, 0, 0, colorImg.Width, colorImg.Height);

                    if (list.count > 0)
                    {
                        AddMsgToTextBox(this.textLog, string.Format("识别到二维码{0}", list.aRet[0].code));

                        return list;
                    }
                    window.ShowImage(colorImg);
                    Cv2.WaitKey(sleepTime);
                }
                if (detectCount > 50)
                    break;
                detectCount++;
                
            }
            Log.E("DetectQRCode", "未识别到二维码");
            return list;
        }
        //PXCMSession session;
        //PXCMSenseManager sm;
        
        public OFFSET DetectFakeSquare(string fingerID)
        {
            OFFSET os = new OFFSET();
            MYPOINT[] ptCur = new MYPOINT[4];
            //var window = new Window("depth");
            
            ptCur = camera_detect(1280, 720, widthInImage, heightInImage, error);

            if (ptCur[0].x != 0 && ptCur[0].y != 0)
            {
                os = Adjust(ptCur);
                os.num = 1;
                Log.I("CameraDetect", String.Format("arc={0:N6},x={1},y={2}", os.arc, os.offsetX, os.offsetY));
                
                return os;
            }
            else
            {
                Log.I("CameraDetect", "未检测到假手指");
            }
            return os;
        }
        
        private object obj = new object();
        public void testMethod()
        {
            //adb logcat
            AppTest.retcode ret = AppTest.retcode.ERROR_MOK;
            if (useAppium)
            {
                /*
                if (appiumServerFlag == false)
                {
                    MessageBox.Show("Appium服务启动失败，请重启服务");
                    return;
                }*/
                
                if (AppTest.Setup() == false)
                {
                    MessageBox.Show("手机连接失败");
                    return;
                }
                ret = AppTest.SaveData();
                AddMsgToTextBox(this.textLog, string.Format("[Client] Save simulate data {0}", ret));
            }

            var workbook = new XLWorkbook();
            //Summary页面
            var workSumsheet = workbook.Worksheets.Add("Summary");
            var window = new Window("capture");
            var capture = new VideoCapture(0);
            //var windowDepth = new Window("depth");
       
            int sleepTime = (int)Math.Round(1000 / capture.Fps);
            //tray盘整盘测试
            AddMsgToTextBox(this.textLogRobot, "StartTest");
            
            lock (obj)
            {
                CreateClient(urPort);
                string feedback;
                //移到tray盘第一格
                Log.I("UR5", "机械臂移到Tray盘第一格");
                string commandAboveInitTray;
                commandAboveInitTray = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                    baseTrayPostion[0], baseTrayPostion[1], baseTrayPostion[2] + aboveTray, baseTrayPostion[3], baseTrayPostion[4], baseTrayPostion[5], moveA, moveV);
                feedback = URController.Send_command_WithFeedback(commandAboveInitTray);
                CloseClient();
                string commandAboveQRCode, commandAboveTray, commandTray;
                for (int m = 0; m < boxRowNum; m++)//start位置 m,n可配置
                {
                    for (int n = 0; n < boxColNum; n++)
                    {
                        AddMsgToTextBox(this.textLogRobot, string.Format("index={0}", m * boxColNum + n));
                        Log.I("UR5", string.Format("index={0}", m * boxColNum + n));
                        //移到待测试假手指二维码上方
                        Log.I("UR5", "摄像头移到待测试假手指二维码上方");
                        
                        CreateClient(urPort);
                        commandAboveQRCode = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                        baseTrayPostion[0] - n * (spaceVertical + spaceBetweenBox) - 0.016, baseTrayPostion[1] + m * (spaceHorizontal + spaceBetweenBox) + 0.0488, 
                        baseTrayPostion[2] + aboveQRCode, baseTrayPostion[3], baseTrayPostion[4], baseTrayPostion[5], moveA, moveV);
                        URController.Send_command_WithFeedback(commandAboveQRCode);
                        
                        string fingerID = "";
                        QRCode.QRCodeList list = DetectQRCode(window, capture, sleepTime);
                        QRCode.QRCodeList listRetry;
                        if (list.count == 1)
                        {
                            fingerID = list.aRet[0].code;
                            Log.I("DetectQRCode", "识别到二维码" + fingerID);
                        }
                        else
                        {
                            if (count == 0)
                            {
                                AddMsgToTextBox(this.textLog, string.Format("index={0} count={1} 识别二维码失败，重试", m * boxColNum + n, list.count));
                                //焦点没对准，重新移动对焦一次
                                Log.I("DetectQRCode", "焦点没对准，重新移动对焦一次");
                                capture.Release();
                                capture = new VideoCapture(0);
                                listRetry = DetectQRCode(window, capture, sleepTime);
                                if (listRetry.count == 1)
                                {
                                    fingerID = listRetry.aRet[0].code;
                                    Log.I("DetectQRCode", "识别到二维码" + fingerID);
                                }
                                else
                                {
                                    fingerID = Log.GetTime();
                                    Log.E("DetectQRCode", "未识别到二维码");
                                }
                            }
                            else
                            {
                                fingerID = Log.GetTime();
                                Log.E("DetectQRCode", "未识别到二维码");
                            }
                        }
                        
                        Log.I("UR5", "机械臂移动到Tray盘假手指上方");
                        commandAboveTray = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                        baseTrayPostion[0] - n * (spaceVertical + spaceBetweenBox), baseTrayPostion[1] + m * (spaceHorizontal + spaceBetweenBox), 
                        baseTrayPostion[2] + aboveTray, baseTrayPostion[3], baseTrayPostion[4], baseTrayPostion[5], moveA, moveV);
                        URController.Send_command_WithFeedback(commandAboveTray);
                            
                        if (useAppium)
                        {
                            //检测app是否正常前置！！！！！！！！！！！
                            
                            ret = AppTest.EntryVerify();
                            AddMsgToTextBox(this.textLog, string.Format("[Client] Enter verify {0}", ret));

                            ret = AppTest.EnterName(fingerID);
                            AddMsgToTextBox(this.textLog, string.Format("[Client] Input name {0} {1}", fingerID, ret));
                            AddMsgToTextBox(this.textLogRobot, string.Format("Input name {0}", fingerID));
                            list.count = 0;
                        }

                        //拾取假手指
                        Log.I("AirPump", "放下吸盘");
                        if (useAirPump)
                            Air.down();

                        Log.I("UR5", "移动到Tray盘待测假手指上方");
                        commandTray = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                            baseTrayPostion[0] - n * (spaceVertical + spaceBetweenBox), baseTrayPostion[1] + m * (spaceHorizontal + spaceBetweenBox),
                            baseTrayPostion[2], baseTrayPostion[3], baseTrayPostion[4], baseTrayPostion[5], moveA, moveV);
                        URController.Send_command_WithFeedback(commandTray);
                        
                        Log.I("AirPump", "吸取假手指");
                        if (useAirPump)
                            Air.pickUp();
                        Thread.Sleep(1000);
                        URController.Send_command_WithFeedback(commandAboveTray);
                        
                        //移到手机上方
                        Log.I("UR5", "移动到光斑上方");
                        string commandAboveFacula = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                            baseFaculaPostion[0], baseFaculaPostion[1], baseFaculaPostion[2] + aboveFacula, 
                            baseFaculaPostion[3], baseFaculaPostion[4], baseFaculaPostion[5], moveA, moveV);
                        URController.Send_command_WithFeedback(commandAboveFacula);
                            
                        count = 0;
                        string pos = "";
                        Log.I("UR5", "开始按压测试");
                            
                        //开始按压测试
                        CloseClient();
                        CreateClient(urPort);
                        moveX(fingerID);
                        CloseClient();
                        CreateClient(urPort);
                        moveY(fingerID);
                        CloseClient();
                        CreateClient(urPort);
                        moveD1(fingerID);
                        CloseClient();
                        CreateClient(urPort);
                            
                        pos = moveD2(fingerID);
                            
                        //记录当前假手指结果
                        sumResult = writeExcel(fingerID, workbook, resList);
                        sumList.Add(sumResult);
                        resList.Clear();//清空假手指结果
                        
                        Log.I("Appium", "获取识别总结果");
                        if (useAppium)
                        {
                            Dictionary<string, int> d = AppTest.GetCount();
                            if (d.Count > 0)
                            {
                                AddMsgToTextBox(this.textLogRobot, string.Format("Total:{0},Success:{1},Failure:{2},Fake:{3}\r\n", d["total"], d["success"], d["failure"], d["Fake"]));
                                Log.I("Appium", "成功获取总识别结果");
                            }
                            else
                            {
                                AddMsgToTextBox(this.textLogRobot, string.Format("Total:未找到测试结果\r\n"));
                                Log.I("Appium", "获取总识别结果失败");
                            }
                        }
                        Log.I("UR5", "深度摄像头移到手机上的假手指上方");
                        //机械臂移动到能检测放在屏幕上的假手指的位置上
                        //上移
                        string commandAboveFake = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                            baseFakePostion[0], baseFakePostion[1], baseFakePostion[2]+0.02, baseFakePostion[3],
                            baseFakePostion[4], baseFakePostion[5], moveA, moveV);
                        URController.Send_command_WithFeedback(commandAboveFake);
                        //摄像头移到假手指上方
                        commandAboveFake = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                            baseFakePostion[0], baseFakePostion[1], baseFakePostion[2], baseFakePostion[3], 
                            baseFakePostion[4], baseFakePostion[5], moveA, moveV);
                        URController.Send_command_WithFeedback(commandAboveFake);

                        //Mat mat = OpenCvSharp.Extensions.BitmapConverter.ToMat(bmEnd);
                        //bmEnd.Save("E:\\x.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                        //检测2d假手指
                        
                        OFFSET os = new OFFSET();
                        os = DetectFakeSquare(fingerID);
                        
                        //吸盘移到假手指上方,用movel旋转
                        double postW = baseFaculaPostionL[5] - os.arc;
                        string commandOffsetFake = string.Format("movel([{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                            baseFaculaPostionL[0], baseFaculaPostionL[1], baseFaculaPostionL[2], baseFaculaPostionL[3],
                            baseFaculaPostionL[4], postW, 0.4, 0.4);
                        URController.Send_command(commandOffsetFake);
                        Thread.Sleep(1000);
                        if(useAirPump)
                            Air.down();
                           
                        Thread.Sleep(1000);//等机械臂移动到commandOffsetFake位置，等URDateHandle.Positions_X读到当前位置
                        if (os.num == 0)
                        {
                            //未检测到假手指
                            URController.Send_command_WithFeedback(pos);//回到假手指最后一次位置
                        }
                        else
                        {
                            double curX = 0;
                            double curY = 0;
                            double curZ = 0;
                            double curU = 0;
                            double curV = 0;
                            double curW = 0;
                            if (URDateHandle.Positions_X != 0 && URDateHandle.Positions_Y != 0)
                            {
                                curX = URDateHandle.Positions_X;
                                curY = URDateHandle.Positions_Y;
                                curZ = URDateHandle.Positions_Z;
                                curU = URDateHandle.Positions_U;
                                curV = URDateHandle.Positions_V;
                                curW = URDateHandle.Positions_W;

                                //用movelp平移
                                double postX = curX - 0.04 * os.offsetX / 390;
                                double postY = curY + 0.04 * os.offsetY / 390;
                                commandOffsetFake = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                                    postX, postY, baseFaculaPostion[2], curU, curV, curW, 0.05, 0.05);
                                URController.Send_command(commandOffsetFake);
                            }
                            else
                            {
                                Log.E("UR5", "未定位到机械臂位置，机械臂位置为0");
                                URController.Send_command_WithFeedback(pos);//回到假手指最后一次位置
                            }
                        }
                        Thread.Sleep(2000);//等待机械臂移到假手指上，等待太少时间未接触到假手指时会吸取失败
                        //吸取假手指
                        Air.pickUp();
                        //把假手指放回Tray盘
                        URController.Send_command_WithFeedback(commandAboveFacula);
                           
                        if (useAppium)
                        {
                            ret = AppTest.BackHome();
                            AddMsgToTextBox(this.textLog, string.Format("[Client] Return back home {0}", ret));
                            //AppTest.Restart();
                        }
                        Log.I("UR5", "把假手指放回Tray盘");
                        URController.Send_command_WithFeedback(commandAboveTray);
                        URController.Send_command_WithFeedback(commandTray);
                        if (useAirPump)
                            Air.drop();
                        URController.Send_command_WithFeedback(commandAboveTray);
 
                        CloseClient();
                        AddMsgToTextBox(this.textLogRobot, "\n");//每个假手指log分开
                        
                        clearMemory();
                       
                    }
                }
                //回到tray盘第一格位置
                CreateClient(urPort);
                feedback = URController.Send_command_WithFeedback(commandAboveInitTray);
                CloseClient();
            }
            
            capture.Release();
            window.Dispose();
            writeSumExcel(workSumsheet, sumList);
            
            workbook.SaveAs(string.Format("{0}测试结果.xlsx", Log.GetTime()));
           
            if (useAppium)
                AppTest.QuitDriver();
            AddMsgToTextBox(this.textLogRobot, "End");

        }
        private void moveX(string fingerID)
        {
            AppTest.retcode ret = AppTest.retcode.ERROR_MOK;
            string command = "";
            int index = 0;
            double tmp = spaceX;
            for (int j = 0; j < 2; j++)//
            {
                for (int i = 0; i < xCount /2; i++)//
                {
                    count++;
                    if (i == 0 && j == 1)
                    {
                        spaceX = -1 * spaceX;
                    }
                    index = i + 1;

                    //移到目标位置上方
                    command = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                    baseFaculaPostion[0] + index * spaceX, baseFaculaPostion[1], baseFaculaPostion[2] + moveUp, baseFaculaPostion[3],
                    baseFaculaPostion[4], baseFaculaPostion[5], moveA, moveUpVSpeed);
                    URController.Send_command_WithFeedback(command);
                    //下移到目标位置
                    command = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                    baseFaculaPostion[0] + index * spaceX, baseFaculaPostion[1], baseFaculaPostion[2], baseFaculaPostion[3],
                    baseFaculaPostion[4], baseFaculaPostion[5], moveA, moveDownVSpeed);
                    URController.Send_command_WithFeedback(command);
                    AddMsgToTextBox(this.textLogRobot, string.Format("{0} ", count) + command);
                    recResult.wc = command;//记录机械臂位置

                    //释放假手指
                    if (useAirPump)
                        Air.drop();
                    Thread.Sleep(500);
                    //上移机械臂
                    command = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                    baseFaculaPostion[0] + index * spaceX, baseFaculaPostion[1], baseFaculaPostion[2] + moveUp, baseFaculaPostion[3],
                    baseFaculaPostion[4], baseFaculaPostion[5], moveA, moveUpVSpeed);
                    URController.Send_command_WithFeedback(command);
                    if (useAirPump)
                        Air.up();//判断吸盘已经收起再继续

                    //下移到光斑位置
                    command = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                    baseFaculaPostion[0], baseFaculaPostion[1], baseFaculaPostion[2] + 0.001, baseFaculaPostion[3],
                    baseFaculaPostion[4], baseFaculaPostion[5], moveA, moveV);
                    URController.Send_command_WithFeedback(command);
                    
                    //按压一次
                    if (useAirPump)
                    {
                        Thread.Sleep(500);
                        Air.pressDown();
                        Thread.Sleep(1000);
                        //Air.pressUp();
                    }

                    
                    //为了不带动纸张，先上移到光斑上方
                    command = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                    baseFaculaPostion[0], baseFaculaPostion[1], baseFaculaPostion[2] + 0.005, baseFaculaPostion[3],
                    baseFaculaPostion[4], baseFaculaPostion[5], 0.01, 0.01);
                    URController.Send_command_WithFeedback(command);
                    
                    //移到目标位置上方
                    command = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                    baseFaculaPostion[0] + index * spaceX, baseFaculaPostion[1], baseFaculaPostion[2] + moveUp, baseFaculaPostion[3],
                    baseFaculaPostion[4], baseFaculaPostion[5], moveA, moveUpVSpeed);
                    URController.Send_command_WithFeedback(command);
                  
                    Air.pressUp();
                    //打印每次测试结果
                    if (useAppium)
                    {
                        ret = AppTest.GetResult();
                        AddMsgToTextBox(this.textLogRobot, TranslateResult(ret));
                        recResult.result = ret;
                        recResult.lc = string.Format("({0},{1})", index * spaceX, 0);
                        if (j == 0)
                            recResult.orient = "X1";
                        else
                            recResult.orient = "X2";
                        resList.Add(recResult);//记录每次按压结果
                    }
                    if (useAirPump)
                        Air.down();
                    
                    //移到假手指位置
                    command = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                    baseFaculaPostion[0] + index * spaceX, baseFaculaPostion[1], baseFaculaPostion[2], baseFaculaPostion[3],
                    baseFaculaPostion[4], baseFaculaPostion[5], moveA, moveDownVSpeed);
                    URController.Send_command_WithFeedback(command);

                    //拾起假手指
                    if (useAirPump)
                    {
                        Thread.Sleep(500);
                        Air.pickUp();//判断已经吸起再继续
                    }
                }
            }
            spaceX = tmp;
        }
        private void moveY(string fingerID)
        {
            AppTest.retcode ret = AppTest.retcode.ERROR_MOK;
            double tmp = spaceY;
            string command = "";
            int index = 0;
            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < yCount / 2; i++)
                {
                    count++;
                    if (i == 0 && j == 1)
                        spaceY = -1 * spaceY;
                    index = i + 1;

                    //移到目标位置上方
                    command = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                    baseFaculaPostion[0], baseFaculaPostion[1] + index * spaceY, baseFaculaPostion[2] + moveUp, baseFaculaPostion[3],
                    baseFaculaPostion[4], baseFaculaPostion[5], moveA, moveUpVSpeed);
                    URController.Send_command_WithFeedback(command);
                    //下移到目标位置
                    command = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                    baseFaculaPostion[0], baseFaculaPostion[1] + index * spaceY, baseFaculaPostion[2], baseFaculaPostion[3],
                    baseFaculaPostion[4], baseFaculaPostion[5], moveA, moveDownVSpeed);
                    URController.Send_command_WithFeedback(command);
                    AddMsgToTextBox(this.textLogRobot, string.Format("{0} ", count) + command);
                    recResult.wc = command;//记录机械臂位置
                    //释放假手指
                    if (useAirPump)
                        Air.drop();
                    Thread.Sleep(500);
                    //上移机械臂
                    command = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                    baseFaculaPostion[0], baseFaculaPostion[1] + index * spaceY, baseFaculaPostion[2] + moveUp, baseFaculaPostion[3],
                    baseFaculaPostion[4], baseFaculaPostion[5], moveA, moveUpVSpeed);
                    URController.Send_command_WithFeedback(command);
                    if (useAirPump)
                        Air.up();

                    //下移到光斑位置
                    command = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                    baseFaculaPostion[0], baseFaculaPostion[1], baseFaculaPostion[2] + 0.001, baseFaculaPostion[3],
                    baseFaculaPostion[4], baseFaculaPostion[5], moveA, moveV);
                    URController.Send_command_WithFeedback(command);
                    
                    //按压一次
                    if (useAirPump)
                    {
                        Thread.Sleep(500);
                        Air.pressDown();
                        Thread.Sleep(1000);
                        
                    }
                    

                    //为了不带动纸张，先上移到光斑上方
                    command = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                    baseFaculaPostion[0], baseFaculaPostion[1], baseFaculaPostion[2] + moveUp, baseFaculaPostion[3],
                    baseFaculaPostion[4], baseFaculaPostion[5], moveA, moveV);
                    URController.Send_command_WithFeedback(command);
                    //移到目标位置上方
                    command = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                    baseFaculaPostion[0], baseFaculaPostion[1] + index * spaceY, baseFaculaPostion[2] + moveUp, baseFaculaPostion[3],
                    baseFaculaPostion[4], baseFaculaPostion[5], moveA, moveUpVSpeed);
                    URController.Send_command_WithFeedback(command);
                    Air.pressUp();

                    if (useAppium)
                    {
                        ret = AppTest.GetResult();
                        AddMsgToTextBox(this.textLogRobot, TranslateResult(ret));
                        //recResult.fingerID = fingerID;
                        recResult.result = ret;
                        recResult.lc = string.Format("({0},{1})", 0, index * spaceY);
                        if (j == 0)
                            recResult.orient = "Y1";
                        else
                            recResult.orient = "Y2";
                        resList.Add(recResult);//记录每次按压结果
                    }
                    if (useAirPump)
                        Air.down();
                    //移到假手指位置
                    command = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                    baseFaculaPostion[0], baseFaculaPostion[1] + index * spaceY, baseFaculaPostion[2], baseFaculaPostion[3],
                    baseFaculaPostion[4], baseFaculaPostion[5], moveA, moveDownVSpeed);
                    URController.Send_command_WithFeedback(command);

                    //拾起假手指
                    if (useAirPump)
                    {
                        Thread.Sleep(500);
                        Air.pickUp();
                    }

                }
            }
            spaceY = tmp;
        }
        private void moveD1(string fingerID)
        {
            AppTest.retcode ret = AppTest.retcode.ERROR_MOK;
            double tmp1 = spaceD1[0];
            double tmp2 = spaceD1[1];
            string command = "";
            int index = 0;
            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < dCount1 / 2; i++)
                {
                    count++;
                    if (i == 0 && j == 1)
                    {
                        for (int k = 0; k < 2; k++)
                        {
                            spaceD1[k] = -1 * spaceD1[k];
                        }
                    }
                    index = i + 1;

                    //移到目标位置上方
                    command = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                    baseFaculaPostion[0] + index * spaceD1[0], baseFaculaPostion[1] + index * spaceD1[1], baseFaculaPostion[2] + moveUp,
                    baseFaculaPostion[3], baseFaculaPostion[4], baseFaculaPostion[5], moveA, moveUpVSpeed);
                    URController.Send_command_WithFeedback(command);
                    //下移到目标位置
                    command = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                    baseFaculaPostion[0] + index * spaceD1[0], baseFaculaPostion[1] + index * spaceD1[1], baseFaculaPostion[2],
                    baseFaculaPostion[3], baseFaculaPostion[4], baseFaculaPostion[5], moveA, moveDownVSpeed);
                    URController.Send_command_WithFeedback(command);
                    AddMsgToTextBox(this.textLogRobot, string.Format("{0} ", count) + command);
                    recResult.wc = command;//记录机械臂位置
                    //释放假手指
                    if (useAirPump)
                        Air.drop();
                    Thread.Sleep(500);
                    //上移机械臂
                    command = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                    baseFaculaPostion[0] + index * spaceD1[0], baseFaculaPostion[1] + index * spaceD1[1], baseFaculaPostion[2] + moveUp,
                    baseFaculaPostion[3], baseFaculaPostion[4], baseFaculaPostion[5], moveA, moveUpVSpeed);
                    URController.Send_command_WithFeedback(command);
                    if (useAirPump)
                        Air.up();

                    //下移到光斑位置
                    command = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                    baseFaculaPostion[0], baseFaculaPostion[1], baseFaculaPostion[2] + 0.001, baseFaculaPostion[3], 
                    baseFaculaPostion[4], baseFaculaPostion[5], moveA, moveV);
                    URController.Send_command_WithFeedback(command);
                    
                    //按压一次
                    if (useAirPump)
                    {
                        Thread.Sleep(500);
                        Air.pressDown();
                        Thread.Sleep(1000);
                        //Air.pressUp();
                    }

                    

                    command = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                    baseFaculaPostion[0], baseFaculaPostion[1], baseFaculaPostion[2] + moveUp, baseFaculaPostion[3],
                    baseFaculaPostion[4], baseFaculaPostion[5], moveA, moveV);
                    URController.Send_command_WithFeedback(command);
                    //移到目标位置上方
                    command = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                    baseFaculaPostion[0] + index * spaceD1[0], baseFaculaPostion[1] + index * spaceD1[1], baseFaculaPostion[2] + moveUp,
                    baseFaculaPostion[3], baseFaculaPostion[4], baseFaculaPostion[5], moveA, moveUpVSpeed);
                    URController.Send_command_WithFeedback(command);
                    Air.pressUp();
                    //打印每次测试结果
                    if (useAppium)
                    {
                        ret = AppTest.GetResult();
                        AddMsgToTextBox(this.textLogRobot, TranslateResult(ret));

                        recResult.result = ret;
                        recResult.lc = string.Format("({0},{1})", index * spaceD1[0], index * spaceD1[1]);
                        if (j == 0)
                            recResult.orient = "D45_1";
                        else
                            recResult.orient = "D45_2";
                        resList.Add(recResult);//记录每次按压结果
                    }

                    if (useAirPump)
                        Air.down();
                    //移到假手指位置
                    command = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                    baseFaculaPostion[0] + index * spaceD1[0], baseFaculaPostion[1] + index * spaceD1[1], baseFaculaPostion[2], 
                    baseFaculaPostion[3], baseFaculaPostion[4], baseFaculaPostion[5], moveA, moveDownVSpeed);
                    URController.Send_command_WithFeedback(command);
                   
                    //拾起假手指
                    if (useAirPump)
                    {
                        Thread.Sleep(500);
                        Air.pickUp();
                    }

                }
            }
            spaceD1[0] = tmp1;
            spaceD1[1] = tmp2;
        }
        private string moveD2(string fingerID)
        {
            AppTest.retcode ret = AppTest.retcode.ERROR_MOK;
            double tmp1 = spaceD2[0];
            double tmp2 = spaceD2[1];
            string command = "";
            int index = 0;

            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < dCount2 / 2; i++)
                {
                    count++;
                    if (i == 0 && j == 1)
                    {
                        for (int k = 0; k < 2; k++)
                        {
                            spaceD2[k] = -1 * spaceD2[k];
                        }
                    }
                    index = i + 1;

                    //移到目标位置上方
                    command = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                    baseFaculaPostion[0] - index * spaceD2[0], baseFaculaPostion[1] + index * spaceD2[1], baseFaculaPostion[2] + moveUp,
                    baseFaculaPostion[3], baseFaculaPostion[4], baseFaculaPostion[5], moveA, moveUpVSpeed);
                    
                    URController.Send_command_WithFeedback(command);
                    //下移到目标位置
                    command = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                    baseFaculaPostion[0] - index * spaceD2[0], baseFaculaPostion[1] + index * spaceD2[1], baseFaculaPostion[2],
                    baseFaculaPostion[3], baseFaculaPostion[4], baseFaculaPostion[5], moveA, moveDownVSpeed);

                    URController.Send_command_WithFeedback(command);
                    AddMsgToTextBox(this.textLogRobot, string.Format("{0} ", count) + command);
                    recResult.wc = command;//记录机械臂位置
                    //释放假手指
                    if (useAirPump)
                        Air.drop();
                    Thread.Sleep(500);
                    //上移机械臂
                    command = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                    baseFaculaPostion[0] - index * spaceD2[0], baseFaculaPostion[1] + index * spaceD2[1], baseFaculaPostion[2] + moveUp,
                    baseFaculaPostion[3], baseFaculaPostion[4], baseFaculaPostion[5], moveA, moveUpVSpeed);
                    URController.Send_command_WithFeedback(command);
                    if (useAirPump)
                        Air.up();

                    //下移到光斑位置
                    command = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                    baseFaculaPostion[0], baseFaculaPostion[1], baseFaculaPostion[2] + 0.001, baseFaculaPostion[3], baseFaculaPostion[4],
                    baseFaculaPostion[5], moveA, moveV);
                    URController.Send_command_WithFeedback(command);
                    
                    //按压一次
                    if (useAirPump)
                    {
                        Thread.Sleep(500);
                        Air.pressDown();
                        Thread.Sleep(1000);
                        //Air.pressUp();
                    }

                    //为了不带动纸张，先上移到光斑上方
                    command = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                    baseFaculaPostion[0], baseFaculaPostion[1], baseFaculaPostion[2] + moveUp, baseFaculaPostion[3],
                    baseFaculaPostion[4], baseFaculaPostion[5], moveA, moveV);
                    URController.Send_command_WithFeedback(command);
                    //移到目标位置上方
                    command = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                    baseFaculaPostion[0] - index * spaceD2[0], baseFaculaPostion[1] + index * spaceD2[1], baseFaculaPostion[2] + moveUp,
                    baseFaculaPostion[3], baseFaculaPostion[4], baseFaculaPostion[5], moveA, moveUpVSpeed);
                    URController.Send_command_WithFeedback(command);
                    Air.pressUp();
                    if (useAirPump)
                        Air.down();
                    //移到假手指位置
                    command = string.Format("movel(p[{0},{1},{2},{3},{4},{5}], a = {6}, v = {7})",
                    baseFaculaPostion[0] - index * spaceD2[0], baseFaculaPostion[1] + index * spaceD2[1], baseFaculaPostion[2],
                    baseFaculaPostion[3], baseFaculaPostion[4], baseFaculaPostion[5], moveA, moveDownVSpeed);
                   
                    //打印每次测试结果
                    if (useAppium)
                    {
                        ret = AppTest.GetResult();
                        AddMsgToTextBox(this.textLogRobot, TranslateResult(ret));

                        recResult.result = ret;
                        recResult.lc = string.Format("({0},{1})", -index * spaceD2[0], index * spaceD2[1]);
                        if (j == 0)
                            recResult.orient = "D135_1";
                        else
                            recResult.orient = "D135_2";
                        resList.Add(recResult);//记录每次按压结果
                    }
                    if (j == 1 && i == dCount2 / 2 - 1)
                    {
                        //最后一次
                        break;
                    }
                    URController.Send_command_WithFeedback(command);

                    //拾起假手指
                    if (useAirPump)
                    {
                        Thread.Sleep(500);
                        Air.pickUp();
                    }
                }
            }

            spaceD2[0] = tmp1;
            spaceD2[1] = tmp2;
            return command;
        }
        private string TranslateResult(AppTest.retcode ret)
        {
            string res = "Other";
            switch (ret)
            {
                case AppTest.retcode.ERROR_FAKE:
                    res = "Fake";
                    break;
                case AppTest.retcode.ERROR_MATCH:
                    res = "Pass";
                    break;
                case AppTest.retcode.ERROR_NOT_MATCH:
                    res = "Fail";
                    break;
                default:
                    break;
            }
            return res;
        }
        public void AddMsgToTextBox(System.Windows.Forms.TextBox textBox, string s)
        {
            textBox.Text += s + "\r\n";
            textBox.SelectionStart = this.textLogRobot.Text.Length;
            textBox.ScrollToCaret();
        }
        
        public void writeSumExcel(IXLWorksheet workSumsheet, List<RecSum> sumList)
        {
            workSumsheet.Cell(1, 1).Value = "序号";
            workSumsheet.Cell(1, 2).Value = "Case名称";
            workSumsheet.Cell(1, 3).Value = "总按压次数";
            workSumsheet.Cell(1, 4).Value = "非真手指";
            workSumsheet.Cell(1, 5).Value = "识别成功";
            workSumsheet.Cell(1, 6).Value = "不匹配";
            workSumsheet.Cell(1, 7).Value = "请用力按压";
            for (int i = 0; i < sumList.Count(); i++)
            {
                workSumsheet.Cell(i + 2, 1).Value = i + 1;
                workSumsheet.Cell(i + 2, 2).Value = sumList[i].fingerID;
                workSumsheet.Cell(i + 2, 3).Value = sumList[i].pressSum;
                workSumsheet.Cell(i + 2, 4).Value = sumList[i].fakeSum;
                workSumsheet.Cell(i + 2, 5).Value = sumList[i].passSum;
                workSumsheet.Cell(i + 2, 6).Value = sumList[i].failSum;
                workSumsheet.Cell(i + 2, 7).Value = sumList[i].lightSum;
            }
        }
        public RecSum writeExcel(string fingerID, XLWorkbook workbook, List<RecRes> resList)
        {
            RecSum sum = new RecSum();
            
            sum.fingerID = fingerID;
            var worksheet = workbook.Worksheets.Add(fingerID);
            worksheet.Cell("A1").Value = "序号";
            worksheet.Cell(1, 2).Value = "Case名称";
            worksheet.Cell(1, 3).Value = "理论坐标";
            worksheet.Cell(1, 4).Value = "实际坐标";
            worksheet.Cell(1, 5).Value = "测试结果";
            worksheet.Cell(1, 6).Value = "方向";
            for (int i = 0; i < resList.Count(); i++)
            {
                worksheet.Cell(i + 2, 1).Value = i + 1;
                worksheet.Cell(i + 2, 2).Value = fingerID;
                worksheet.Cell(i + 2, 3).Value = resList[i].lc;
                worksheet.Cell(i + 2, 4).Value = resList[i].wc;
                worksheet.Cell(i + 2, 5).Value = resList[i].result;
                worksheet.Cell(i + 2, 6).Value = resList[i].orient;

                sum.pressSum++;
                switch (resList[i].result)
                {
                    case AppTest.retcode.ERROR_FAKE:
                        sum.fakeSum++;
                        break;
                    case AppTest.retcode.ERROR_MATCH:
                        sum.passSum++;
                        break;
                    case AppTest.retcode.ERROR_NOT_MATCH:
                        sum.failSum++;
                        break;
                    case AppTest.retcode.ERROR_LIGHT:
                        sum.lightSum++;
                        break;
                    default:
                        break;
                }
            }
            return sum;
        }
        private bool CheckExe(string exeName)
        {
            int counter = 0;
            foreach (Process process in Process.GetProcessesByName(exeName))
            {
                counter++;
            }
            if (counter > 0)
                return true;
            else
                return false;
        }
        private void CloseExe(string exeName)
        {
            foreach (Process process in Process.GetProcessesByName(exeName))
            {
                process.Kill();
            }
        }
        private void RunCmd(object obj)//string cmdExe
        {
            try
            {
                using (Process process = new Process())
                {
                    Control.CheckForIllegalCrossThreadCalls = false;
                    process.StartInfo.FileName = "cmd.exe";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardInput = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;
                    process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
                    process.Start();
                    //如果调用程序路径中有空格时，cmd命令执行失败，可以用双引号括起来 ，在这里两个引号表示一个引号（转义）
                    //string cmdExe = "taskkill";
                    //string cmdStr = "/F /IM node.exe";
                    string str = obj.ToString();//string.Format(@"{0} {1}", cmdExe, cmdStr);

                    process.StandardInput.WriteLine(str);//必须加上exit，WaitForExit才能及时退出
                    process.StandardInput.AutoFlush = true;
                    process.BeginOutputReadLine();
                    if (str.IndexOf("exit") != -1)
                    {
                        process.WaitForExit();
                        process.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Log.E("RunCmd",e.ToString());
                MessageBox.Show(e.ToString());
            }
        }
        private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                if (outLine.Data.Contains("[Appium]"))
                {
                    if (outLine.Data.Contains("Welcome to Appium"))
                    {
                        appiumBtnFlag = true;
                    }

                    if (outLine.Data.Contains("Appium REST http interface listener started on"))
                    {
                        //appiumServerFlag = true;
                    }

                    StringBuilder sb = new StringBuilder(this.textLog.Text);
                    this.textLog.Text = sb.AppendLine(outLine.Data).ToString();
                    this.textLog.SelectionStart = this.textLog.Text.Length;//每次刷新显示最后输出字符
                    this.textLog.ScrollToCaret();
                    Console.WriteLine(outLine.Data);
                }
            }
        }
        private void OpenAirPumpExe(string path, string cmd)
        {
            //指定启动进程是调用的应用程序和命令行参数
            //using (Process process = new Process())
            {
                myPro.StartInfo.WorkingDirectory = path;//必须指定在此目录下运行
                myPro.StartInfo.FileName = path + cmd;
                myPro.StartInfo.Verb = "Open";
                myPro.Start();

            }
        }
        /*
        public void OpenDepthCamera()
        {
            session = PXCMSession.CreateInstance();
            sm = session.CreateSenseManager();

            //开启视频流（有三个视频数据流1颜色2深度3红外，下面是选择的颜色）
            sm.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_COLOR, 1280, 720);

            //图像的宽度和高度0为默认
            sm.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_DEPTH, 640, 480);

            sm.Init();//启动初始
            //获取初始化状态异常处理
        }
        public void CloseDepthCamera()
        {
            if (sm != null)
            {
                sm.Close();//关闭会话
            }
        }
        public Bitmap GetOneDepthFrame()//出现黑色图像？
        {
            Bitmap bm = new Bitmap(1280, 720);
            for (int i = 0; i < 10; i++)
            {
                if (sm.AcquireFrame(true) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
                {
                    //定义并初始化一个捕获对象
                    PXCMCapture.Sample sample = sm.QuerySample();

                    //获取对象中的颜色数据
                    PXCMImage image = sample.color;
                    PXCMImage dimage = sample.depth;
                    //定义一个输出颜色数据
                    PXCMImage.ImageData colorData;

                    //使用捕获对象将数据存放到colordata中
                    image.AcquireAccess(PXCMImage.Access.ACCESS_READ, PXCMImage.PixelFormat.PIXEL_FORMAT_RGB24, out colorData);

                    //将输出数据转化为bmp图片
                    //WriteableBitmap wbm = colorData.ToWritableBitmap(0, image2.info.width, image2.info.height, 72.0, 72.0);

                    //将bmp图片显示在image控件中。
                    bm = colorData.ToBitmap(0, image.info.width, image.info.height);//new Bitmap(dimage.info.width, dimage.info.height, PixelFormat.Format16bppRgb555);
                                                                                    //cameraImage.Image = bm;
                                                                                    //cameraImage.DrawToBitmap(bm, new Rectangle(0, 0, bm.Width, bm.Height));
                                                                                    //bm.Save(string.Format("E:\\save\\{0}.png", index), ImageFormat.Png);
                    sm.ReleaseFrame();//关闭视频
                                      //bm.Save(string.Format("E:\\{0}.bmp", i), System.Drawing.Imaging.ImageFormat.Bmp);
                }
                else
                {
                    //MessageBox.Show("Open camera failed");
                    
                    Log.E("DepthCamera", "Open camera failed");
                    CloseDepthCamera();
                    OpenDepthCamera();
                }
            }
            return bm;
        }*/
        protected override void OnFormClosing(FormClosingEventArgs e)//关闭窗口按钮
        {/*
            DialogResult result = MessageBox.Show("是否退出", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                //改写Toml里的state.AirPump状态
                //CloseDepthCamera();
                Stop();
                RunCmd("taskkill /F /IM node.exe&exit");
                if (useAirPump)
                    Air.close();
                
                //this.Dispose();//子窗口用此句
            }
            else
            {
                e.Cancel = true;
            }*/
        }
        private void UpdateToml(string key, bool value)
        {
            //Toml.Serializer.Write<var>(doc, "state", "");
            //string cmd = string.Format("python {0} {1} {2} {3}&exit", pyPath, tomlPath, key, value);
            //RunCmd(cmd);
        }
    }
}
