using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;



namespace ConsoleApplication1
{
    class Program
    {
        //RGBBitmap转灰度BitMap
        static Bitmap GetGrayImage(Bitmap srcBmp)
        {
            Rectangle rect = new Rectangle(0, 0, srcBmp.Width, srcBmp.Height);
            BitmapData srcBmpData = srcBmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);//用Bitmapdata对bitmap做操作
            IntPtr srcPtr = srcBmpData.Scan0;//初始位置指针
            int scanWidth = srcBmpData.Width * 3;//扫描宽度，每个像素三通道，故乘3
            int src_bytes = scanWidth * srcBmp.Height;//总字节数
            byte[] srcRGBValues = new byte[src_bytes];//建立一维byte数组储存RGB图像数据
            Marshal.Copy(srcPtr, srcRGBValues, 0, src_bytes);
            //灰度化处理  
            int k = 0;
            for (int i = 0; i < srcBmp.Height; i++)
            {
                for (int j = 0; j < srcBmp.Width; j++)
                {
                    k = j * 3;
                    //0.299*R + 0.587*G + 0.144*B = 亮度或灰度  
                    //只处理每行中图像像素数据,舍弃未用空间  
                    //注意位图结构中RGB按BGR的顺序存储  
                    byte intensity = (byte)(srcRGBValues[i * scanWidth + k + 2] * 0.299//R
                         + srcRGBValues[i * scanWidth + k + 1] * 0.587//G
                         + srcRGBValues[i * scanWidth + k + 0] * 0.114);//B
                    srcRGBValues[i * scanWidth + k + 0] = intensity;//B
                    srcRGBValues[i * scanWidth + k + 1] = intensity;//G
                    srcRGBValues[i * scanWidth + k + 2] = intensity;//R
                }
            }
            Marshal.Copy(srcRGBValues, 0, srcPtr, src_bytes);
            //解锁位图  
            srcBmp.UnlockBits(srcBmpData);
            return srcBmp;
        }
        //Bitmap转灰度二值数组
        static byte[,] GetGrayArray2D(Bitmap srcBmp)
        {
            Rectangle rect = new Rectangle(0, 0, srcBmp.Width, srcBmp.Height);
            BitmapData srcBmpData = srcBmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int width = rect.Width;
            int height = rect.Height;

           
            IntPtr srcPtr = srcBmpData.Scan0;

            int scanWidth = width * 3;
            int src_bytes = scanWidth * height;
            //int srcStride = srcBmpData.Stride;  
            byte[] srcRGBValues = new byte[src_bytes];
            byte[,] grayValues = new byte[height, width];
            //RGB[] rgb = new RGB[srcBmp.Width * rows];  
            //复制GRB信息到byte数组  
            Marshal.Copy(srcPtr, srcRGBValues, 0, src_bytes);
            //解锁位图  
            srcBmp.UnlockBits(srcBmpData);
            //灰度化处理  
            int m = 0, i = 0, j = 0;  //m表示行，j表示列  
            int k = 0;
            byte gray;

            for (i = 0; i < height; i++)  //只获取图片的rows行像素值  
            {
                for (j = 0; j < width; j++)
                {
                    //只处理每行中图像像素数据,舍弃未用空间  
                    //注意位图结构中RGB按BGR的顺序存储  
                    k = 3 * j;
                    gray = (byte)(srcRGBValues[i * scanWidth + k + 2] * 0.299
                         + srcRGBValues[i * scanWidth + k + 1] * 0.587
                         + srcRGBValues[i * scanWidth + k + 0] * 0.114);

                    grayValues[m, j] = gray;  //将灰度值存到double的数组中  
                }
                m++;
            }

            return grayValues;
        }
        static Boolean NonZero(Int32 value)
        {
            return (value != 0) ? true : false;
        }
        static Int32 OtsuThreshold(Byte[,] grayarray)
        {
            Int32[] Histogram = new int[256];
            Array.Clear(Histogram, 0, 256);
            foreach (Byte b in grayarray)// 统计直方图
            {
                Histogram[b]++;
            }
            Int32 SumC = grayarray.Length;//像素点个数
            Double SumU = 0;
            for (Int32 i = 1; i < 256; i++)
            {
                SumU += i * Histogram[i];
            }
            Int32 minGrayLevel = Array.FindIndex(Histogram, NonZero);
            Int32 maxGrayLevel = Array.FindLastIndex(Histogram, NonZero);

            Int32 Threshold = minGrayLevel;
            Double MaxVariance = 0;//初始最大方差
            Double U0 = 0;//初始目标质量矩
            Int32 C0 = 0;//初始目标点数
            for(Int32 i=minGrayLevel;i<maxGrayLevel;i++)
            {
                if (Histogram[i] == 0)
                    continue;
                //目标质量矩和点数
                U0 += i * Histogram[i];
                C0 += Histogram[i];
                Double Difference = U0 * SumC - SumU * C0;
                Double Variance = Difference * Difference / C0 / (SumC - C0);
                if (Variance > MaxVariance)
                {
                    MaxVariance = Variance;
                    Threshold = i;
                }
            }
            return Threshold;
        }
        static void Thresholding(Bitmap rsc, Int32 threshold)
        {
            for (int i = 0; i < rsc.Width; i++)
            {
                for (int j = 0; j < rsc.Height; j++)
                {
                    Color pixelColor = rsc.GetPixel(i, j);
                    if (pixelColor.R > threshold) rsc.SetPixel(i, j, Color.FromArgb(255, 255, 255));
                    else rsc.SetPixel(i, j, Color.FromArgb(0, 0, 0));
                }
            }
        }

        static void Main(string[] args)
        {
            Bitmap map = new Bitmap("D:/Documents/Projects/C#/ConsoleApplication1/ConsoleApplication1/test.png");
            GetGrayImage(map);
            byte[,] GrayArray = GetGrayArray2D(map);
            Int32 Threshold = OtsuThreshold(GrayArray);
            Thresholding(map,Threshold);
            map.Save("test.jpg", ImageFormat.Jpeg);
        }
    }
}