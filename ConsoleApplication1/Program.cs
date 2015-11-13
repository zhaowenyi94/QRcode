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

        //Bitmap转灰度二维数组
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

        //判断非零项
        static Boolean NonZero(Int32 value)
        {
            return (value != 0) ? true : false;
        }
        
        //大通发求滤波阀值
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

        //带阀值全局二值化
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
        //找到坐标点
        static void Processing(Bitmap curBitmap)
        {
            if (curBitmap != null)
            {
                double[,] process = new double[curBitmap.Height, curBitmap.Width];
                for (int j = 0; j < curBitmap.Height; j++)
                {
                    int i = 0;
                    int counter = -1;
                    int sum = 1;
                    while (i < curBitmap.Width)
                    {
                        while (i < curBitmap.Width - 1 && (curBitmap.GetPixel(i, j)).R == (curBitmap.GetPixel(i + 1, j)).R)
                        {
                            sum = sum + 1;
                            i = i + 1;
                        }
                        counter = counter + 1;
                        process[j, counter] = sum;
                        sum = 1;
                        i = i + 1;
                    }
                }

                //得到了process
                double[,] suitratio = new double[1000, 1000];
                int sum1 = 0;//sum1是之前的横坐标数目
                for (int row = 0; row < curBitmap.Height; row++)
                {
                    for (int col = 0; col < curBitmap.Width - 4; col++)
                    {
                        double[] select = new double[5];
                        for (int i = 0; i < 5; i++)
                        {
                            select[i] = process[row, col + i];
                        }

                        double max = select.Max();
                        double selectsum = select.Sum();
                        double baselen = (selectsum - max) / 4;
                        if (baselen != 0)
                        {
                            if (select[0] >= baselen * 0.5 && select[0] <= baselen * 1.5 && select[1] >= baselen * 0.5 && select[1] <= baselen * 1.5 && select[2] >= baselen * 2.1 && select[2] <= baselen * 3.9 && select[3] >= baselen * 0.5 && select[3] <= baselen * 1.5 && select[4] >= baselen * 0.5 && select[4] <= baselen * 1.5)
                            {
                                for (int k = 0; k < col + 2; k++)
                                    sum1 += (int)process[row, k];
                                if (select[2] % 2 == 1)
                                {
                                    sum1 = sum1 + (int)(select[2] / 2) + 1;
                                    if (curBitmap.GetPixel(sum1, row).R == 0)
                                    {
                                        suitratio[row, sum1] = 1;
                                    }
                                }
                                else
                                {
                                    sum1 = sum1 + (int)(select[2] / 2);
                                    if (curBitmap.GetPixel(sum1, row).R == 0)
                                    {
                                        suitratio[row, sum1] = 1;
                                    }
                                    sum1 = sum1 + 1;
                                    if (curBitmap.GetPixel(sum1, row).R == 0)
                                    {
                                        suitratio[row, sum1] = 1;
                                    }
                                }
                            }
                        }
                        sum1 = 0;
                    }
                }//end of  process过后的 行处理


                double[,] process1 = new double[1000, 1000];
                for (int j = 0; j < curBitmap.Width; j++)
                {
                    int i = 0;
                    int counter = -1;
                    int sum = 1;
                    while (i < curBitmap.Height)
                    {
                        while ((i < curBitmap.Height - 1) && (curBitmap.GetPixel(j, i)).R == (curBitmap.GetPixel(j, i + 1)).R)
                        {
                            sum = sum + 1;
                            i = i + 1;
                        }
                        counter = counter + 1;
                        process1[counter, j] = sum;
                        sum = 1;
                        i = i + 1;
                    }
                }

                //得到了process1
                sum1 = 0;//sum1是之前的横坐标数目
                for (int col = 0; col < curBitmap.Width; col++)
                {
                    for (int row = 0; row < curBitmap.Height - 4; row++)
                    {
                        double[] select = new double[5];
                        for (int i = 0; i < 5; i++)
                        {
                            select[i] = process1[row + i, col];
                        }
                        double max = select.Max();
                        double selectsum = select.Sum();
                        double baselen = (selectsum - max) / 4;
                        if (baselen != 0)
                        {
                            if (select[0] >= baselen * 0.5 && select[0] <= baselen * 1.5 && select[1] >= baselen * 0.5 && select[1] <= baselen * 1.5 && select[2] >= baselen * 2.1 && select[2] <= baselen * 3.9 && select[3] >= baselen * 0.5 && select[3] <= baselen * 1.5 && select[4] >= baselen * 0.5 && select[4] <= baselen * 1.5)
                            {
                                for (int k = 0; k < row + 2; k++)
                                    sum1 = sum1 + (int)process1[k, col];
                                if (select[2] % 2 == 1)
                                {
                                    sum1 = sum1 + (int)(select[2] / 2) + 1;
                                    if (curBitmap.GetPixel(col, sum1).R == 0)
                                    {
                                        suitratio[sum1, col] = suitratio[sum1, col] + 1;
                                    }
                                }
                                else
                                {
                                    sum1 = sum1 + (int)(select[2] / 2);
                                    if (curBitmap.GetPixel(col, sum1).R == 0)
                                    {
                                        suitratio[sum1, col] = suitratio[sum1, col] + 1;
                                    }
                                    sum1 = sum1 + 1;
                                    if (curBitmap.GetPixel(col, sum1).R == 0)
                                    {
                                        suitratio[sum1, col] = suitratio[sum1, col] + 1;
                                    }
                                }
                            }
                        }
                        sum1 = 0;
                    }
                }//end of  process过后的 行处理
                double co_x;
                double co_y;
                int count = -1;
                double[,] point = new double[3, 2];//之后单独写宏定义，在检测时输入有几个被监测的二维码
                for (int row = 0; row < curBitmap.Height; row++)
                {
                    for (int col = 0; col < curBitmap.Width; col++)
                    {
                        if (suitratio[row, col] == 2)
                        {
                            if (suitratio[row + 1, col] == 2 && suitratio[row, col + 1] == 2)
                            {
                                suitratio[row + 1, col] = 0;
                                suitratio[row, col + 1] = 0;
                                suitratio[row + 1, col + 1] = 0;
                                co_x = col + 0.5;
                                co_y = row + 0.5;
                            }
                            else if (suitratio[row + 1, col] == 2)
                            {
                                suitratio[row + 1, col] = 0;
                                co_x = col;
                                co_y = row + 0.5;
                            }
                            else if (suitratio[row, col + 1] == 2)
                            {
                                suitratio[row, col + 1] = 0;
                                suitratio[row + 1, col + 1] = 0;
                                co_x = col + 0.5;
                                co_y = row;
                            }
                            else { co_x = col; co_y = row; }
                            count = count + 1;
                            point[count, 0] = co_x;
                            point[count, 1] = co_y;
                            Graphics g;
                            g = Graphics.FromImage(curBitmap); //创建画板
                            Rectangle rg = new Rectangle(col - 5, row - 5, 10, 10); //填充区域
                            g.FillRectangle(new SolidBrush(Color.Red), rg); //填充黑色
                        }
                    }

                }
                String str = "coordinate:\n";
                for (int row = 0; row < 3; row++)
                    for (int col = 0; col < 2; col++)
                    {
                        String str1 = point[row, col].ToString();
                        if (col == 0)
                            str = str + "\n" + str1;
                        else
                            str = str + "  " + str1;
                    }
                Console.WriteLine(str);
            }
        }




        //主函数
        static void Main(string[] args)
        {
            Bitmap map = new Bitmap("D:/Documents/Projects/C#/ConsoleApplication1/ConsoleApplication1/test.png");
            GetGrayImage(map);
            byte[,] GrayArray = GetGrayArray2D(map);
            Int32 Threshold = OtsuThreshold(GrayArray);
            Thresholding(map,Threshold);
            Processing(map);
            map.Save("test.jpg", ImageFormat.Jpeg);
        }
    }
}