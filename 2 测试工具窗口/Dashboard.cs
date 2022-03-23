﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using URControl;
using Files;

namespace UR_点动控制器
{
    public partial class Dashboard : Form
    {
        public string DefaultINIPath;

        public Dashboard(string ConfigFilePath)
        {
            InitializeComponent();
            DefaultINIPath = ConfigFilePath;
        }

        //这次不是502端口，也不是30003端口，而是29999端口
        URControlHandle URController = new URControlHandle();

        //除了一个随时待命的29999端口，还需要一个实时的29999的socekt客户端，随时从UR获取所需要的数据
        URControlHandle URStateMonitor = new URControlHandle();

        //在这里申明一个定时器，用于实时获取UR数据
        public System.Timers.Timer GetRobotInformationTimer;

        private void Dashboard_Load(object sender, EventArgs e)
        {
            FilesINI ConfigController = new FilesINI();
            string Target_IP = ConfigController.INIRead("UR控制参数", "RemoteIP", DefaultINIPath);
            int Control_Port = 29999;

            //创建Dashboard客户端
            URController.Creat_client(Target_IP, Control_Port);

            //一开始连接到UR之后UR会主动发过来一条信息
            string Feedback = URController.No_command_WaitFeedback();
            txtFeedback.Items.Add(Feedback);

            //设置这个Combobox
            UserRoleBox.Items.Add("程序员");
            UserRoleBox.Items.Add("操作员");
            UserRoleBox.Items.Add("完全锁定");
            

            //在新的线程中始终查看当前的机器人状态
            URStateMonitor.Creat_client(Target_IP, Control_Port);
            initialRobotMonitor();
        }

        //这里就不把时间做到变量里了，每隔一定时间获取数据，假定就是100ms
        private void initialRobotMonitor()
        {
            //调用定时器，在线程中定时执行任务
            GetRobotInformationTimer = new System.Timers.Timer(100);
            GetRobotInformationTimer.Elapsed += new System.Timers.ElapsedEventHandler(GetRobotState);
            GetRobotInformationTimer.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；
            GetRobotInformationTimer.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
        }

        public void GetRobotState(object source, System.Timers.ElapsedEventArgs e)
        {
            //定时器里只添加一个获取Robotmode的命令
            string Feedback = URStateMonitor.Send_command_WithFeedback("Robotmode");
            txtURState.Text = Feedback;
        }



        private void btnRunbtnRun_Click(object sender, EventArgs e)
        {
            //其实任何命令都是有反馈信息的。问题只是你要不要接收罢了
            //URController.Send_command("play");
            string Feedback = URController.Send_command_WithFeedback("play");
            txtFeedback.Items.Add(Feedback);

        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            //URController.Send_command("pause");
            string Feedback = URController.Send_command_WithFeedback("pause");
            txtFeedback.Items.Add(Feedback);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            //URController.Send_command("stop");
            string Feedback = URController.Send_command_WithFeedback("stop");
            txtFeedback.Items.Add(Feedback);
        }

        private void btnShutdown_Click(object sender, EventArgs e)
        {
            //URController.Send_command("shutdown");
            string Feedback = URController.Send_command_WithFeedback("shutdown");
            txtFeedback.Items.Add(Feedback);
        }

        private void btnGetCurrentProgram_Click(object sender, EventArgs e)
        {

            string Feedback = URController.Send_command_WithFeedback("get loaded program");
            txtFeedback.Items.Add(Feedback);
        
        }

        private void btnLoadCurrentProgram_Click(object sender, EventArgs e)
        {
            string NewProgram = txtProgramPath.Text;
            string Feedback = URController.Send_command_WithFeedback("load" + NewProgram);
            txtFeedback.Items.Add(Feedback);
        }

        private void btnSendCommand_Click(object sender, EventArgs e)
        {
            //发送自定义命令一定要接收反馈，因为如果你不收，UR还是会把反馈放到Socket里面，下次你再收，还会有上次的Socket残留信息
            string Feedback = URController.Send_command_WithFeedback(txtCustomCommand.Text);
            txtFeedback.Items.Add(Feedback);
            
        }

        //加入右键菜单好用
        private void RightMenu_Copy_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(this.txtFeedback.SelectedItem.ToString());  
        }

        private void RightMenu_Delete_Click(object sender, EventArgs e)
        {
            this.txtFeedback.Items.Remove(this.txtFeedback.SelectedItem);
        }

        private void RightMenu_Clear_Click(object sender, EventArgs e)
        {
            this.txtFeedback.Items.Clear();
        }

        //这项功能只有在最新版的UR PolyScope才支持（from version 1.8, revision 11657 ）
        private void ChangeRole(object sender, EventArgs e)
        {
            //MessageBox.Show(this.UserRoleBox.SelectedItem.ToString());
            String Role = this.UserRoleBox.SelectedItem.ToString();
            if (Role == "程序员")
            {
                string Feedback = URController.Send_command_WithFeedback("setUserRole <programmer >");
                txtFeedback.Items.Add(Feedback);
            }
            else if (Role == "操作员")
            {
                string Feedback = URController.Send_command_WithFeedback("setUserRole <operator>");
                txtFeedback.Items.Add(Feedback);
            }
            else
            {
                string Feedback = URController.Send_command_WithFeedback("setUserRole <locked>");
                txtFeedback.Items.Add(Feedback);
            }


        }

        private void btnGetCurrentState_Click(object sender, EventArgs e)
        {
            string Feedback = URController.Send_command_WithFeedback("robotmode");
            txtFeedback.Items.Add(Feedback);
        }


    }
}
