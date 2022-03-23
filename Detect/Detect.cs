using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
//using OpenCvSharp;
using System.Drawing;
//using System.Drawing.Imaging;

using Config;
using LogManage;

namespace Detect
{
    public class SquareDetect
    {
        public static int flag = 0;
        public struct MYPOINT
        {
            public int x;
            public int y;
        }
      
        [DllImport("CameraDetect.dll", EntryPoint = "CameraDetect")]
        public static extern int CameraDetect(int imgWith, int imgHeight, int with, int height, int error, IntPtr ptPt, IntPtr pName);
        public static OFFSET DetectFakeSquare(string resName)
        {
            OFFSET os = new OFFSET();
            MYPOINT[] ptArr = new MYPOINT[4];

            int size = Marshal.SizeOf(typeof(MYPOINT)) * 4;
            IntPtr ptPtr = Marshal.AllocHGlobal(size);

            IntPtr pName = Marshal.StringToHGlobalAnsi(resName);
            int n = CameraDetect(1280, 720, ConfigParam.widthInImage, ConfigParam.heightInImage, ConfigParam.error, ptPtr, pName);
            if (n > 0)
            {
                Log.I("CameraDetect", "当前假手指位置：");
                for (int i = 0; i < 4; i++)
                {
                    IntPtr ptr = new IntPtr(ptPtr.ToInt64() + Marshal.SizeOf(typeof(MYPOINT)) * i);
                    ptArr[i] = (MYPOINT)Marshal.PtrToStructure(ptr, typeof(MYPOINT));
                    Log.I("CameraDetect", string.Format("{0},{1}", ptArr[i].x, ptArr[i].y));
                }
                
                os = Adjust(ptArr);
                os.num = 1;
                Log.I("CameraDetect", String.Format("arc={0:N6},x={1},y={2}", os.arc, os.offsetX, os.offsetY));
            }
            else
            {
                Log.I("CameraDetect", "未检测到假手指");
            }
            Marshal.FreeHGlobal(pName);
            Marshal.FreeHGlobal(ptPtr);
            return os;
        }
        [DllImport("ImageDetect.dll", EntryPoint = "DetectSquare")]
        public static extern int ImageDetectSquare(IntPtr imgPtr, int imgWith, int imgHeight, int with, int height, int error, IntPtr ptPtr, int flag);//检测截屏图
        public static MYPOINT[] image_detect(Bitmap bm, int width, int height, int error)
        {
            //Bitmap bm = (Bitmap)System.Drawing.Image.FromFile("D:\\0_res1.png");
     
            int size = Marshal.SizeOf(typeof(MYPOINT)) * 4;
            IntPtr ptPtr = Marshal.AllocHGlobal(size);
            Rectangle rect = new Rectangle(0, 0, bm.Width, bm.Height);
            System.Drawing.Imaging.BitmapData bmpData = bm.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, bm.PixelFormat);
            IntPtr imgPtr = bmpData.Scan0;
            int n = ImageDetectSquare(imgPtr, bm.Width, bm.Height, width, height, error, ptPtr, flag);
            flag = 1;
            bm.UnlockBits(bmpData);

            MYPOINT[] ptArr = new MYPOINT[4];
            if (n > 0)
            {
                Log.I("ImageDetect", "当前矩形框位置：");
                for (int i = 0; i < 4; i++)
                {
                    IntPtr ptr = new IntPtr(ptPtr.ToInt64() + Marshal.SizeOf(typeof(MYPOINT)) * i);
                    ptArr[i] = (MYPOINT)Marshal.PtrToStructure(ptr, typeof(MYPOINT));
                    Log.I("ImageDetect", string.Format("{0},{1}", ptArr[i].x, ptArr[i].y));
                }
            }
            Marshal.FreeHGlobal(ptPtr);
            return ptArr;
        }
        public struct OFFSET
        {
            public int offsetX;
            public int offsetY;
            public double arc;
            public int num;
        }
        public static OFFSET Adjust(MYPOINT[] ptCur)
        {
            OFFSET os = new OFFSET();
            //原始位置PositionInImage,现在位置ptCur
            //以左上和右下为准：(ptStd[2],ptStd[3])，(ptStd[6],ptStd[7])
            //求中心点位移
            System.Drawing.Point ptStdCenter = new System.Drawing.Point();
            System.Drawing.Point ptCurCenter = new System.Drawing.Point();

            ptStdCenter.X = ConfigParam.ptStd[2] + (ConfigParam.ptStd[6] - ConfigParam.ptStd[2]) / 2;
            ptStdCenter.Y = ConfigParam.ptStd[3] + (ConfigParam.ptStd[7] - ConfigParam.ptStd[3]) / 2;

            ptCurCenter.X = ptCur[1].x + (ptCur[3].x - ptCur[1].x) / 2;
            ptCurCenter.Y = ptCur[1].y + (ptCur[3].y - ptCur[1].y) / 2;
            if (ptCurCenter.X == 0 && ptCurCenter.Y == 0)
                return os;
            int moveX = ptCurCenter.X - ptStdCenter.X;
            int moveY = ptCurCenter.Y - ptStdCenter.Y;

            int w = (int)Math.Sqrt(Math.Pow((ptCur[0].x - ptCur[1].x), 2) + Math.Pow((ptCur[0].y - ptCur[1].y), 2));
            int h = (int)Math.Sqrt(Math.Pow((ptCur[0].x - ptCur[3].x), 2) + Math.Pow((ptCur[0].y - ptCur[3].y), 2));
            double arcValue = 0;
            //Console.WriteLine("wxh:{0}x{1}",w,h);
            if (w > h)
            {
                double tanValue = 0;
                if (ptCur[0].y - ptCur[1].y == 0)
                {
                    arcValue = 0;
                }
                else
                {
                    tanValue = Math.Abs((double)(ptCur[0].y - ptCur[1].y) / (ptCur[0].x - ptCur[1].x));//tan值
                    arcValue = -Math.Atan(tanValue);

                }
            }
            else
            {
                double tanValue = 0;
                if (ptCur[0].y - ptCur[1].y == 0)
                {
                    arcValue = 0;
                }
                else
                {
                    tanValue = Math.Abs((double)(ptCur[0].x - ptCur[1].x) / (ptCur[0].y - ptCur[1].y));//tan值
                    arcValue = Math.Atan(tanValue);

                }
            }
            //double angleValue = arcValue / Math.PI * 180;//角度
            os.offsetX = moveX;
            os.offsetY = moveY;
            os.arc = arcValue;
            return os;
        }
    }
}
