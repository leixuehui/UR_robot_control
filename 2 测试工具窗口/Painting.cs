﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.IO;

namespace UR_点动控制器
{
    public partial class Painting : Form
    {

        public Painting()
        {
            InitializeComponent();
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
        }

        //设置全局的前景色和背景色(还有绘图的线宽)
        public Color foreColor = Color.Red;
        public Color backColor = Color.Green;
        public int LineWidth = 3;

        //设置起始点和终止点
        public static Point startPoint, oldPoint;

        //设置全局操作的位图
        public Image theImage;

        //设置当前文件的文件名
        public string editFileName;

        //绘制位图的Graphics实例
        public Graphics ig;

        //设置全局的是否在绘图(0表示不在画图，1表示在画图)
        public bool isDrawing = false;

        //设置是否记录点位输出
        public bool ifRecordByPoint = false;
        public bool ifRecordByTime = false;

        //设置两个字符串
        string RecordByPointStr = "";
        string RecordByTimeStr = "";

        //默认采用二维数组来保存XY的点位和当前是否鼠标按下（设定一个极限的记录点位的数量）
        public static int PointRecordsTick = 0;//从0开始
        public static int PointRecordLimit = 10000;//设置记录的极限容量
        public static int[,] PointRecords = new int[PointRecordLimit, 3];

        public Graphics g;
        private void Painting_Load(object sender, EventArgs e)
        {

        }

        private void ToolStrip_New_Click(object sender, EventArgs e)
        {
            //修改绘图控件为picturebox
            //g = this.CreateGraphics();
            g = this.pictureBox1.CreateGraphics();
            

            g.Clear(backColor);

            //创建一个Bitmap（取决于当前实际工作区域的大小）
            //theImage = new Bitmap(this.ClientRectangle.Width, this.ClientRectangle.Height/2);
            theImage = new Bitmap(980, 400);
            
            //MessageBox.Show("Width:" + this.ClientRectangle.Width.ToString() + "Height:" + this.ClientRectangle.Height.ToString());
            editFileName = "新建文件";
            ig = Graphics.FromImage(theImage);
            ig.Clear(backColor);

            //重置所有参数
            PointRecordsTick = 0;//从0开始
            PointRecords = new int[PointRecordLimit, 3];
        }

        //查看点位输出
        private void Output_Points_Output_Click(object sender, EventArgs e)
        {
            //先清空原有文本
            textboxCommand.Text = "";

            //获取比例系数
            double Percent = Convert.ToDouble(textBox_Percent.Text);

            //只显示鼠标轨迹坐标点(注意最后一行不要了)
            //否则又会出现下面这种问题，第二行代码运行会直接死机
            /*
                movel(pose_add(HomeVectorTruely, p[0.00162,-0.00630,0.01013,0,0,0]),v = 0.03, r = 0.001)
                movel(pose_add(HomeVectorTruely, p[0.00000,0.00000,0.00000,0,0,0]),v = 0, r = 0)
                movel(HomeVector,v = 0.1234567,r = 0) 
             */
            for (int i = 0; i < (PointRecordsTick); i++)
            {
                if(i != (PointRecordsTick-1))
                {
                    RecordByPointStr = "X" + ((double)PointRecords[i, 0] * Percent).ToString("0.00") + " Y" + ((double)PointRecords[i, 1] * Percent).ToString("0.00") + " Z" + ((double)PointRecords[i, 2] * Percent).ToString("0.00") + "\r\n";
                }
                else
                {
                    RecordByPointStr = "X" + ((double)PointRecords[i, 0] * Percent).ToString("0.00") + " Y" + ((double)PointRecords[i, 1] * Percent).ToString("0.00") + " Z" + ((double)PointRecords[i, 2] * Percent).ToString("0.00");
                }
                textboxCommand.AppendText(RecordByPointStr);
            }

        }

        //保存为类似于G代码的文件
        private void Output_Points_Save_Click(object sender, EventArgs e)
        {
            //弹出保存文件对话框
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "保存当前点位";//对话框标题
            sfd.InitialDirectory = Convert.ToString(System.AppDomain.CurrentDomain.BaseDirectory);//对话框初始目录
            sfd.Filter = "TXT文件|*.txt";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                //保存当前配置(sfd.FileName就已经保存了当前文件名的绝对路径)
                //MessageBox.Show(sfd.FileName);
                FileStream fs1 = new FileStream(sfd.FileName, FileMode.Append, FileAccess.Write);//用追加的方式最好
                StreamWriter sw = new StreamWriter(fs1);
                sw.Write(textboxCommand.Text);
                sw.Close();
                fs1.Close();
            }
        }

        private void Output_Image_Save_Click(object sender, EventArgs e)
        {
            //弹出保存文件对话框
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "保存当前点位";//对话框标题
            sfd.InitialDirectory = Convert.ToString(System.AppDomain.CurrentDomain.BaseDirectory);//对话框初始目录
            sfd.Filter = "PNG文件|*.png";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                theImage.Save(sfd.FileName);
            }

        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            //始终记录（显示在窗口标题中）
            this.Text = "MouseX = " + e.X.ToString() + "MouseY = " + e.Y.ToString() + "isDrawing = " + isDrawing.ToString() + "PointRecordsTick/PointRecordLimit = " + PointRecordsTick.ToString() + "/" + PointRecordLimit.ToString();

            //只要点击了根据点位驱动记录开始（存入TXT文件中的一条记录）
            if (ifRecordByPoint)
            {
                //在记录之前先判断是否超出最大记录数
                if (PointRecordsTick > (PointRecordLimit - 1))
                {
                    PointRecordsTick = PointRecordLimit;
                    this.Text = "已经超出了最大记录条数,请右键结束绘制！";
                }
                else
                {
                    //当前记录点位的第一个是当前的X坐标
                    PointRecords[PointRecordsTick, 0] = e.X;
                    PointRecords[PointRecordsTick, 1] = e.Y;

                    //如果在画图，则为200，否则为100（仿G代码的高度差）
                    if (isDrawing)
                    {
                        PointRecords[PointRecordsTick, 2] = 100;
                    }
                    else
                    {
                        PointRecords[PointRecordsTick, 2] = 150;
                    }
                }

                PointRecordsTick++;
            }

            if (isDrawing)
            {
                //修改绘图控件为picturebox
                //g = this.CreateGraphics();
                g = this.pictureBox1.CreateGraphics();

                //从上一个点到当前点绘制线段
                g.DrawLine(new Pen(foreColor, LineWidth), oldPoint, new Point(e.X, e.Y));
                ig.DrawLine(new Pen(foreColor, LineWidth), oldPoint, new Point(e.X, e.Y));
                oldPoint.X = e.X;
                oldPoint.Y = e.Y;
            }
        }

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            //如果是鼠标右键，则结束绘制
            if (e.Button == MouseButtons.Right)
            {
                //MessageBox.Show("Over！");
                ifRecordByPoint = false;
            }

            //如果鼠标左键没有双击，则不记录

            //而是在鼠标按下之后，判断
            if ((isDrawing = !isDrawing) == true)
            {
                //如果是鼠标左键按下，则开始记录点位
                if (e.Button == MouseButtons.Left)
                {
                    startPoint = new Point(e.X, e.Y);
                    oldPoint = new Point(e.X, e.Y);
                }

            }
        }

        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
                isDrawing = false;
                //ig.DrawLine(new Pen(foreColor, LineWidth), startPoint, new Point(e.X, e.Y));
        }

        private void PictureBox_MousePaint(object sender, PaintEventArgs e)
        {
            //修改绘图控件为picturebox
            //g = this.CreateGraphics();
            g = this.pictureBox1.CreateGraphics();

            if (theImage != null)
            {
                g.Clear(Color.White);
                g.DrawImage(theImage, this.ClientRectangle);
            }
        }

        private void PictureBox_DoubleClick(object sender, EventArgs e)
        {
            //MessageBox.Show("Start");
            //点击了按钮让这个阀门被打开，即鼠标左键按下就被触发，虽然会记录点位，但是左键没被按下，只是在纸张上方运动而已
            ifRecordByPoint = true;
        }

        //实际窗口被拖拉几次，都会有图像变形的问题，最好的方法还是把当前图像保存起来，然后载入，然后对比
        private void Output_Image_Load_Click(object sender, EventArgs e)
        {
            this.pictureBox1.Image = null;
            OpenFileDialog Openfile = new OpenFileDialog();
            if (Openfile.ShowDialog() == DialogResult.OK)
            {
                //MessageBox.Show(Openfile.FileName);
                this.pictureBox1.Image = Image.FromFile(Openfile.FileName);//读取图片放在pictuBox1
            }
        }

    }
}
