using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OpenCvSharp;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace QRCodeDetect
{
    public class QRCode
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct CodePoint
        {
            public int x;
            public int y;
        }
        public struct QRCodeRet
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
            public string code;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public CodePoint[] ptArr;
        }
        public struct QRCodeList
        {
            public int count;
            public QRCodeRet[] aRet;
        }
        
        [DllImport("qrcode.dll", EntryPoint = "WeChatQRCode")]
        public static extern int WeChatQRCode(IntPtr imgPtr, int imWidth, int imHeight, int left, int top, int width, int height, IntPtr pv);
        
        public static QRCodeList qrcode_detect(Mat img, int imWidth, int imHeight, int left, int top, int width, int height)
        {
            Bitmap bm = new Bitmap(img.Cols, img.Rows, (int)img.Step(), PixelFormat.Format24bppRgb, img.Data);
            int size = Marshal.SizeOf(typeof(QRCodeRet)) * 20;
            IntPtr pBuff = Marshal.AllocHGlobal(size);
            //Bitmap bm = new Bitmap("E:\\26.png");
            
            Rectangle rect = new Rectangle(0, 0, bm.Width, bm.Height);
            System.Drawing.Imaging.BitmapData bmpData = bm.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, bm.PixelFormat);
            
            IntPtr imgPtr = bmpData.Scan0;

            int len = Math.Abs(bmpData.Stride) * bm.Height;
            byte[] pixel = new byte[len];
            Marshal.Copy(imgPtr, pixel, 0, len);

            //int count = WeChatQRCode(imgPtr, bm.Width, bm.Height, 521, 39, 230, 184, pBuff);
            int count = WeChatQRCode(imgPtr, bm.Width, bm.Height, left, top, width, height, pBuff);
            bm.UnlockBits(bmpData);
            

            QRCodeRet[] pClass = new QRCodeRet[20];
            for (int i = 0; i < 20; i++)
            {
                IntPtr ptr = new IntPtr(pBuff.ToInt64() + Marshal.SizeOf(typeof(QRCodeRet)) * i);
                pClass[i] = (QRCodeRet)Marshal.PtrToStructure(ptr, typeof(QRCodeRet));

            }
            Marshal.FreeHGlobal(pBuff);
            QRCodeList list = new QRCodeList();
            
            list.count = count;
            list.aRet = new QRCodeRet[list.count];
            for (int i = 0; i < list.count; i++)
            {
                list.aRet[i] = pClass[i];
            }
            return list;
        }
       
    }
}
