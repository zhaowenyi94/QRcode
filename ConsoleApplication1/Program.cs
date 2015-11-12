using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;



namespace ConsoleApplication1
{
    class Program
    {
        static void toGrey(Bitmap rsc)//grey
        {
            for (int i = 0; i < rsc.Width; i++)
            {
                for (int j = 0; j < rsc.Height; j++)
                {
                    Color pixelColor = rsc.GetPixel(i, j);
                    int grey = (int)(0.299 * pixelColor.R + 0.587 * pixelColor.G + 0.114 * pixelColor.B);
                    Color newColor = Color.FromArgb(grey, grey, grey);
                    rsc.SetPixel(i, j, newColor);
                }
            }
        }

        //binarilize
        static void Thresholding(Bitmap rsc)
        {
            int[] histogram = new int[256];
            int minGrayValue = 255, maxGrayValue = 0;
            for (int i = 0; i< rsc.Width; i++)
            {
                for (int j = 0; j < rsc.Height; j++)
                {
                    Color pixelColor = rsc.GetPixel(i, j);
                    histogram[pixelColor.R]++;//here we choose R cause they are all the same
                    if (pixelColor.R > maxGrayValue) maxGrayValue = pixelColor.R;
                    if (pixelColor.R < minGrayValue) minGrayValue = pixelColor.R;
                }
            }
            int threshold = -1;
            int newThreshold = (minGrayValue + maxGrayValue) / 2;
            for (int iterationTimes = 0; threshold != newThreshold && iterationTimes < 100; iterationTimes++)
            {
                threshold = newThreshold;
                int lP1 = 0;
                int lP2 = 0;
                int lS1 = 0;
                int lS2 = 0;
                for (int i = minGrayValue; i < threshold; i++)
                {
                    lP1 += histogram[i] * i;
                    lS1 += histogram[i];
                }
                int mean1GrayValue = (lP1 / lS1);
                for (int i = threshold + 1; i < maxGrayValue; i++)
                {
                    lP2 += histogram[i] * i;
                    lS2 += histogram[i];                
                }
                int mean2GrayValue = (lP2 / lS2);
                newThreshold = (mean1GrayValue + mean2GrayValue) / 2;
            }
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
            toGrey(map);
            Thresholding(map);
            map.Save("test.jpg", ImageFormat.Jpeg);
        }
    }
}