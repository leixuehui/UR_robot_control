namespace UR_点动控制器
{
    partial class Torso
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtIPAddressPC = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtPortPCToRobotA = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtIPAddressRobotA = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtIPAddressRobotB = new System.Windows.Forms.TextBox();
            this.btnInitial = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.txtPortRobotA = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtPortRobotB = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnTestRobotA = new System.Windows.Forms.Button();
            this.btnTestRobotB = new System.Windows.Forms.Button();
            this.btnRobotAGoZeroPoint = new System.Windows.Forms.Button();
            this.btnRobotBGoZeroPoint = new System.Windows.Forms.Button();
            this.listBoxFromRobotA = new System.Windows.Forms.ListBox();
            this.listBoxFromRobotB = new System.Windows.Forms.ListBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.txtPortPCToRobotB = new System.Windows.Forms.TextBox();
            this.btnClearFeedA = new System.Windows.Forms.Button();
            this.btnClearFeedB = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.txtRobotACommand = new System.Windows.Forms.TextBox();
            this.txtRobotBCommand = new System.Windows.Forms.TextBox();
            this.btnRunJob1 = new System.Windows.Forms.Button();
            this.btnRunjob2 = new System.Windows.Forms.Button();
            this.btnRunjob3 = new System.Windows.Forms.Button();
            this.btnRunjob123 = new System.Windows.Forms.Button();
            this.btnRobotAStop = new System.Windows.Forms.Button();
            this.btnRobotBStop = new System.Windows.Forms.Button();
            this.btnSendPackageToRobotA = new System.Windows.Forms.Button();
            this.btnSendPackageToRobotB = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // txtIPAddressPC
            // 
            this.txtIPAddressPC.Location = new System.Drawing.Point(416, 8);
            this.txtIPAddressPC.Name = "txtIPAddressPC";
            this.txtIPAddressPC.Size = new System.Drawing.Size(123, 21);
            this.txtIPAddressPC.TabIndex = 0;
            this.txtIPAddressPC.Text = "192.168.1.2";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(309, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "当前PC的IP地址：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(309, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "对A机的通信端口：";
            // 
            // txtPortPCToRobotA
            // 
            this.txtPortPCToRobotA.Location = new System.Drawing.Point(428, 31);
            this.txtPortPCToRobotA.Name = "txtPortPCToRobotA";
            this.txtPortPCToRobotA.Size = new System.Drawing.Size(111, 21);
            this.txtPortPCToRobotA.TabIndex = 3;
            this.txtPortPCToRobotA.Text = "8888";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(130, 375);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(107, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "当前A机的IP地址：";
            // 
            // txtIPAddressRobotA
            // 
            this.txtIPAddressRobotA.Location = new System.Drawing.Point(237, 372);
            this.txtIPAddressRobotA.Name = "txtIPAddressRobotA";
            this.txtIPAddressRobotA.Size = new System.Drawing.Size(123, 21);
            this.txtIPAddressRobotA.TabIndex = 6;
            this.txtIPAddressRobotA.Text = "192.168.1.8";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(457, 375);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(107, 12);
            this.label4.TabIndex = 9;
            this.label4.Text = "当前B机的IP地址：";
            // 
            // txtIPAddressRobotB
            // 
            this.txtIPAddressRobotB.Location = new System.Drawing.Point(564, 372);
            this.txtIPAddressRobotB.Name = "txtIPAddressRobotB";
            this.txtIPAddressRobotB.Size = new System.Drawing.Size(123, 21);
            this.txtIPAddressRobotB.TabIndex = 8;
            this.txtIPAddressRobotB.Text = "192.168.1.6";
            // 
            // btnInitial
            // 
            this.btnInitial.Location = new System.Drawing.Point(332, 190);
            this.btnInitial.Name = "btnInitial";
            this.btnInitial.Size = new System.Drawing.Size(107, 29);
            this.btnInitial.TabIndex = 10;
            this.btnInitial.Text = "PC开始监听";
            this.btnInitial.UseVisualStyleBackColor = true;
            this.btnInitial.Click += new System.EventHandler(this.btnInitial_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(130, 399);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(119, 12);
            this.label5.TabIndex = 12;
            this.label5.Text = "测试的A机通信端口：";
            // 
            // txtPortRobotA
            // 
            this.txtPortRobotA.Location = new System.Drawing.Point(249, 396);
            this.txtPortRobotA.Name = "txtPortRobotA";
            this.txtPortRobotA.Size = new System.Drawing.Size(111, 21);
            this.txtPortRobotA.TabIndex = 11;
            this.txtPortRobotA.Text = "29999";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(457, 399);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(119, 12);
            this.label6.TabIndex = 14;
            this.label6.Text = "测试的B机通信端口：";
            // 
            // txtPortRobotB
            // 
            this.txtPortRobotB.Location = new System.Drawing.Point(576, 396);
            this.txtPortRobotB.Name = "txtPortRobotB";
            this.txtPortRobotB.Size = new System.Drawing.Size(111, 21);
            this.txtPortRobotB.TabIndex = 13;
            this.txtPortRobotB.Text = "29999";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::UR_点动控制器.Properties.Resources.Torso3;
            this.pictureBox1.InitialImage = null;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(800, 500);
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            // 
            // btnTestRobotA
            // 
            this.btnTestRobotA.Location = new System.Drawing.Point(132, 427);
            this.btnTestRobotA.Name = "btnTestRobotA";
            this.btnTestRobotA.Size = new System.Drawing.Size(105, 29);
            this.btnTestRobotA.TabIndex = 15;
            this.btnTestRobotA.Text = "手动控制RobotA";
            this.btnTestRobotA.UseVisualStyleBackColor = true;
            this.btnTestRobotA.Click += new System.EventHandler(this.btnTestRobotA_Click);
            // 
            // btnTestRobotB
            // 
            this.btnTestRobotB.Location = new System.Drawing.Point(459, 427);
            this.btnTestRobotB.Name = "btnTestRobotB";
            this.btnTestRobotB.Size = new System.Drawing.Size(111, 29);
            this.btnTestRobotB.TabIndex = 16;
            this.btnTestRobotB.Text = "手动控制RobotB";
            this.btnTestRobotB.UseVisualStyleBackColor = true;
            this.btnTestRobotB.Click += new System.EventHandler(this.btnTestRobotB_Click);
            // 
            // btnRobotAGoZeroPoint
            // 
            this.btnRobotAGoZeroPoint.Location = new System.Drawing.Point(136, 190);
            this.btnRobotAGoZeroPoint.Name = "btnRobotAGoZeroPoint";
            this.btnRobotAGoZeroPoint.Size = new System.Drawing.Size(93, 29);
            this.btnRobotAGoZeroPoint.TabIndex = 17;
            this.btnRobotAGoZeroPoint.Text = "RobotA运行";
            this.btnRobotAGoZeroPoint.UseVisualStyleBackColor = true;
            this.btnRobotAGoZeroPoint.Click += new System.EventHandler(this.btnRobotAGoZeroPoint_Click);
            // 
            // btnRobotBGoZeroPoint
            // 
            this.btnRobotBGoZeroPoint.Location = new System.Drawing.Point(541, 190);
            this.btnRobotBGoZeroPoint.Name = "btnRobotBGoZeroPoint";
            this.btnRobotBGoZeroPoint.Size = new System.Drawing.Size(93, 29);
            this.btnRobotBGoZeroPoint.TabIndex = 18;
            this.btnRobotBGoZeroPoint.Text = "RobotB运行";
            this.btnRobotBGoZeroPoint.UseVisualStyleBackColor = true;
            this.btnRobotBGoZeroPoint.Click += new System.EventHandler(this.btnRobotBGoZeroPoint_Click);
            // 
            // listBoxFromRobotA
            // 
            this.listBoxFromRobotA.FormattingEnabled = true;
            this.listBoxFromRobotA.ItemHeight = 12;
            this.listBoxFromRobotA.Location = new System.Drawing.Point(132, 96);
            this.listBoxFromRobotA.Name = "listBoxFromRobotA";
            this.listBoxFromRobotA.Size = new System.Drawing.Size(158, 88);
            this.listBoxFromRobotA.TabIndex = 19;
            // 
            // listBoxFromRobotB
            // 
            this.listBoxFromRobotB.FormattingEnabled = true;
            this.listBoxFromRobotB.ItemHeight = 12;
            this.listBoxFromRobotB.Location = new System.Drawing.Point(476, 96);
            this.listBoxFromRobotB.Name = "listBoxFromRobotB";
            this.listBoxFromRobotB.Size = new System.Drawing.Size(158, 88);
            this.listBoxFromRobotB.TabIndex = 20;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(134, 76);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(95, 12);
            this.label7.TabIndex = 21;
            this.label7.Text = "来自A机的反馈：";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(481, 76);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(95, 12);
            this.label8.TabIndex = 22;
            this.label8.Text = "来自B机的反馈：";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(309, 56);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(107, 12);
            this.label9.TabIndex = 24;
            this.label9.Text = "对B机的通信端口：";
            // 
            // txtPortPCToRobotB
            // 
            this.txtPortPCToRobotB.Location = new System.Drawing.Point(428, 53);
            this.txtPortPCToRobotB.Name = "txtPortPCToRobotB";
            this.txtPortPCToRobotB.Size = new System.Drawing.Size(111, 21);
            this.txtPortPCToRobotB.TabIndex = 23;
            this.txtPortPCToRobotB.Text = "8889";
            // 
            // btnClearFeedA
            // 
            this.btnClearFeedA.Location = new System.Drawing.Point(235, 72);
            this.btnClearFeedA.Name = "btnClearFeedA";
            this.btnClearFeedA.Size = new System.Drawing.Size(55, 21);
            this.btnClearFeedA.TabIndex = 25;
            this.btnClearFeedA.Text = "清空";
            this.btnClearFeedA.UseVisualStyleBackColor = true;
            this.btnClearFeedA.Click += new System.EventHandler(this.btnClearFeedA_Click);
            // 
            // btnClearFeedB
            // 
            this.btnClearFeedB.Location = new System.Drawing.Point(582, 72);
            this.btnClearFeedB.Name = "btnClearFeedB";
            this.btnClearFeedB.Size = new System.Drawing.Size(52, 21);
            this.btnClearFeedB.TabIndex = 26;
            this.btnClearFeedB.Text = "清空";
            this.btnClearFeedB.UseVisualStyleBackColor = true;
            this.btnClearFeedB.Click += new System.EventHandler(this.btnClearFeedB_Click);
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(332, 225);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(107, 29);
            this.btnReset.TabIndex = 27;
            this.btnReset.Text = "PC重置监听";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // txtRobotACommand
            // 
            this.txtRobotACommand.Location = new System.Drawing.Point(249, 432);
            this.txtRobotACommand.Name = "txtRobotACommand";
            this.txtRobotACommand.Size = new System.Drawing.Size(111, 21);
            this.txtRobotACommand.TabIndex = 28;
            this.txtRobotACommand.Text = "job1";
            // 
            // txtRobotBCommand
            // 
            this.txtRobotBCommand.Location = new System.Drawing.Point(576, 432);
            this.txtRobotBCommand.Name = "txtRobotBCommand";
            this.txtRobotBCommand.Size = new System.Drawing.Size(111, 21);
            this.txtRobotBCommand.TabIndex = 29;
            this.txtRobotBCommand.Text = "job1";
            // 
            // btnRunJob1
            // 
            this.btnRunJob1.Location = new System.Drawing.Point(132, 462);
            this.btnRunJob1.Name = "btnRunJob1";
            this.btnRunJob1.Size = new System.Drawing.Size(105, 29);
            this.btnRunJob1.TabIndex = 30;
            this.btnRunJob1.Text = "同时运行job1";
            this.btnRunJob1.UseVisualStyleBackColor = true;
            this.btnRunJob1.Click += new System.EventHandler(this.btnRunJob1_Click);
            // 
            // btnRunjob2
            // 
            this.btnRunjob2.Location = new System.Drawing.Point(253, 462);
            this.btnRunjob2.Name = "btnRunjob2";
            this.btnRunjob2.Size = new System.Drawing.Size(105, 29);
            this.btnRunjob2.TabIndex = 31;
            this.btnRunjob2.Text = "同时运行job2";
            this.btnRunjob2.UseVisualStyleBackColor = true;
            this.btnRunjob2.Click += new System.EventHandler(this.btnRunjob2_Click);
            // 
            // btnRunjob3
            // 
            this.btnRunjob3.Location = new System.Drawing.Point(373, 462);
            this.btnRunjob3.Name = "btnRunjob3";
            this.btnRunjob3.Size = new System.Drawing.Size(105, 29);
            this.btnRunjob3.TabIndex = 32;
            this.btnRunjob3.Text = "同时运行job3";
            this.btnRunjob3.UseVisualStyleBackColor = true;
            this.btnRunjob3.Click += new System.EventHandler(this.btnRunjob3_Click);
            // 
            // btnRunjob123
            // 
            this.btnRunjob123.Location = new System.Drawing.Point(496, 462);
            this.btnRunjob123.Name = "btnRunjob123";
            this.btnRunjob123.Size = new System.Drawing.Size(191, 29);
            this.btnRunjob123.TabIndex = 33;
            this.btnRunjob123.Text = "连续模式（job1/job2/job3）";
            this.btnRunjob123.UseVisualStyleBackColor = true;
            this.btnRunjob123.Click += new System.EventHandler(this.btnRunjob123_Click);
            // 
            // btnRobotAStop
            // 
            this.btnRobotAStop.Location = new System.Drawing.Point(136, 225);
            this.btnRobotAStop.Name = "btnRobotAStop";
            this.btnRobotAStop.Size = new System.Drawing.Size(93, 29);
            this.btnRobotAStop.TabIndex = 34;
            this.btnRobotAStop.Text = "RobotA停止";
            this.btnRobotAStop.UseVisualStyleBackColor = true;
            this.btnRobotAStop.Click += new System.EventHandler(this.btnRobotAStop_Click);
            // 
            // btnRobotBStop
            // 
            this.btnRobotBStop.Location = new System.Drawing.Point(541, 225);
            this.btnRobotBStop.Name = "btnRobotBStop";
            this.btnRobotBStop.Size = new System.Drawing.Size(93, 29);
            this.btnRobotBStop.TabIndex = 35;
            this.btnRobotBStop.Text = "RobotB停止";
            this.btnRobotBStop.UseVisualStyleBackColor = true;
            this.btnRobotBStop.Click += new System.EventHandler(this.btnRobotBStop_Click);
            // 
            // btnSendPackageToRobotA
            // 
            this.btnSendPackageToRobotA.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.btnSendPackageToRobotA.Location = new System.Drawing.Point(12, 273);
            this.btnSendPackageToRobotA.Name = "btnSendPackageToRobotA";
            this.btnSendPackageToRobotA.Size = new System.Drawing.Size(107, 88);
            this.btnSendPackageToRobotA.TabIndex = 36;
            this.btnSendPackageToRobotA.Text = "往A机发送数据包";
            this.btnSendPackageToRobotA.UseVisualStyleBackColor = false;
            // 
            // btnSendPackageToRobotB
            // 
            this.btnSendPackageToRobotB.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.btnSendPackageToRobotB.Location = new System.Drawing.Point(665, 273);
            this.btnSendPackageToRobotB.Name = "btnSendPackageToRobotB";
            this.btnSendPackageToRobotB.Size = new System.Drawing.Size(107, 88);
            this.btnSendPackageToRobotB.TabIndex = 37;
            this.btnSendPackageToRobotB.Text = "往B机发送数据包";
            this.btnSendPackageToRobotB.UseVisualStyleBackColor = false;
            // 
            // Torso
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(802, 500);
            this.Controls.Add(this.btnSendPackageToRobotB);
            this.Controls.Add(this.btnSendPackageToRobotA);
            this.Controls.Add(this.btnRobotBStop);
            this.Controls.Add(this.btnRobotAStop);
            this.Controls.Add(this.btnRunjob123);
            this.Controls.Add(this.btnRunjob3);
            this.Controls.Add(this.btnRunjob2);
            this.Controls.Add(this.btnRunJob1);
            this.Controls.Add(this.txtRobotBCommand);
            this.Controls.Add(this.txtRobotACommand);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.btnClearFeedB);
            this.Controls.Add(this.btnClearFeedA);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.txtPortPCToRobotB);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.listBoxFromRobotB);
            this.Controls.Add(this.listBoxFromRobotA);
            this.Controls.Add(this.btnRobotBGoZeroPoint);
            this.Controls.Add(this.btnRobotAGoZeroPoint);
            this.Controls.Add(this.btnTestRobotB);
            this.Controls.Add(this.btnTestRobotA);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtPortRobotB);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtPortRobotA);
            this.Controls.Add(this.btnInitial);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtIPAddressRobotB);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtIPAddressRobotA);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtPortPCToRobotA);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtIPAddressPC);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Torso";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Torso";
            this.Load += new System.EventHandler(this.Torso_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtIPAddressPC;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtPortPCToRobotA;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtIPAddressRobotA;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtIPAddressRobotB;
        private System.Windows.Forms.Button btnInitial;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtPortRobotA;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtPortRobotB;
        private System.Windows.Forms.Button btnTestRobotA;
        private System.Windows.Forms.Button btnTestRobotB;
        private System.Windows.Forms.Button btnRobotAGoZeroPoint;
        private System.Windows.Forms.Button btnRobotBGoZeroPoint;
        private System.Windows.Forms.ListBox listBoxFromRobotA;
        private System.Windows.Forms.ListBox listBoxFromRobotB;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtPortPCToRobotB;
        private System.Windows.Forms.Button btnClearFeedA;
        private System.Windows.Forms.Button btnClearFeedB;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.TextBox txtRobotACommand;
        private System.Windows.Forms.TextBox txtRobotBCommand;
        private System.Windows.Forms.Button btnRunJob1;
        private System.Windows.Forms.Button btnRunjob2;
        private System.Windows.Forms.Button btnRunjob3;
        private System.Windows.Forms.Button btnRunjob123;
        private System.Windows.Forms.Button btnRobotAStop;
        private System.Windows.Forms.Button btnRobotBStop;
        private System.Windows.Forms.Button btnSendPackageToRobotA;
        private System.Windows.Forms.Button btnSendPackageToRobotB;
    }
}