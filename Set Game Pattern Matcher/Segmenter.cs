using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;

namespace ScottClayton.Image
{
    /// <summary>
    /// Provides methods for preprocessing an image and then segmenting it.
    /// </summary>
    public class Segmenter
    {
        /// <summary>
        /// The CAPTCHA that all preprocessing operations are applied on.
        /// You can access this image directly from the Segmentation Event if you wish to perform some custom processing.
        /// </summary>
        public Bitmap Image { get; set; }

        /// <summary>
        private static Random random = new Random();

        /// <summary>
        /// Fill a color into a region of an image within a certain tolerance. A random color will be used.
        /// </summary>
        /// <param name="origin">The point to start filling from</param>
        /// <param name="tolerance">The amount of difference naighboring pixels can have and still be considered part of the same group</param>
        public Color FloodFill(Point origin, int tolerance)
        {
            Color fill = Color.FromArgb(random.Next(20, 225), random.Next(20, 225), random.Next(20, 225));
            FloodFill(origin, tolerance, fill);
            return fill;
        }

        /// <summary>
        /// Flood fill from a given point in the image.
        /// </summary>
        private Color FloodFill(Point origin, int tolerance, ref bool[,] filledSquares)
        {
            Color fill = Color.FromArgb(random.Next(20, 225), random.Next(20, 225), random.Next(20, 225));
            FloodFill(origin, tolerance, fill, ref filledSquares);
            return fill;
        }

        /// <summary>
        /// Fill a color into a region of an image within a certain tolerance.
        /// </summary>
        /// <param name="origin">The point to start filling from</param>
        /// <param name="tolerance">The amount of difference naighboring pixels can have and still be considered part of the same group</param>
        /// <param name="fillColor">The color to fill with</param>
        public void FloodFill(Point origin, int tolerance, Color fillColor)
        {
            try
            {
                bool[,] filledSquares = new bool[Image.Width, Image.Height];
                FloodFill(origin, tolerance, fillColor, ref filledSquares);
            }
            catch (Exception ex)
            {
                throw new Exception("Error trying to flood fill from a point on an image.", ex);
            }
        }

        /// <summary>
        /// Flood fill from a given point in the image.
        /// </summary>
        private void FloodFill(Point origin, int tolerance, Color fillColor, ref bool[,] filledSquares)
        {
            Color initialColor = Image.GetPixel(origin.X, origin.Y);
            BitmapData bmpData = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            IntPtr Scan0 = bmpData.Scan0;

            unsafe
            {
                byte* scan0 = (byte*)(void*)Scan0;
                int stride = bmpData.Stride;

                bool[,] doneChecking = new bool[Image.Width, Image.Height];
                Queue<Point> nextPoints = new Queue<Point>();

                // Fill the initial pixel
                FloodFillPoint(scan0, stride, origin, Image.Width, Image.Height, initialColor, tolerance, fillColor, doneChecking, nextPoints, ref filledSquares);

                // Fill pixels in the queue until the queue is empty
                while (nextPoints.Count > 0)
                {
                    Point next = nextPoints.Dequeue();
                    FloodFillPoint(scan0, stride, next, Image.Width, Image.Height, initialColor, tolerance, fillColor, doneChecking, nextPoints, ref filledSquares);
                }
            }

            Image.UnlockBits(bmpData);
        }

        ///// <summary>
        ///// Using the flood fill algorithm, count the number of pixels that WOULD have been filled if we were actually filling.
        ///// Great for counting the number of pixels in a blob.
        ///// </summary>
        ///// <param name="origin">The place to start counting from</param>
        ///// <param name="tolerance">How far off a pixel can be and still be counted as part of the same group</param>
        ///// <param name="cutoff">If you count this many pixels, then stop counting. NOTE: This will prevent a valid bounding box from being returned!</param>
        ///// <returns></returns>
        //private BlobCount FloodCount(Point origin, int tolerance, int cutoff = -1, bool[,] doneChecking = null)
        //{
        //    Color initialColor = Image.GetPixel(origin.X, origin.Y);
        //    BitmapData bmpData = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        //    IntPtr Scan0 = bmpData.Scan0;
        //    int count = 0;
        //    Point upperLeft = origin;
        //    Point lowerRight = origin;

        //    unsafe
        //    {
        //        byte* scan0 = (byte*)(void*)Scan0;
        //        int stride = bmpData.Stride;

        //        if (doneChecking == null)
        //        {
        //            doneChecking = new bool[Image.Width, Image.Height];
        //        }
        //        Queue<Point> nextPoints = new Queue<Point>();

        //        // Fill the initial pixel
        //        FloodFillPoint(scan0, stride, origin, Image.Width, Image.Height, initialColor, tolerance, Color.White, doneChecking, nextPoints, ref doneChecking, true);

        //        // Fill pixels in the queue until the queue is empty
        //        while (nextPoints.Count > 0)
        //        {
        //            Point next = nextPoints.Dequeue();
        //            if (FloodFillPoint(scan0, stride, next, Image.Width, Image.Height, initialColor, tolerance, Color.White, doneChecking, nextPoints, ref doneChecking, true))
        //            {
        //                upperLeft.X = Math.Min(upperLeft.X, next.X);
        //                upperLeft.Y = Math.Min(upperLeft.Y, next.Y);
        //                lowerRight.X = Math.Max(lowerRight.X, next.X);
        //                lowerRight.Y = Math.Max(lowerRight.Y, next.Y);

        //                count++;
        //            }

        //            if (cutoff > 0 && count > cutoff)
        //            {
        //                break;
        //            }
        //        }
        //    }

        //    Image.UnlockBits(bmpData);

        //    return new BlobCount() { PixelCount = count, BlobBounds = new Rectangle(upperLeft.X, upperLeft.Y, lowerRight.X - upperLeft.X, lowerRight.Y - upperLeft.Y) };
        //}

        /// <summary>
        /// Flood fill a certain color starting at a given point using a Breadth First Search (BFS).
        /// </summary>
        private unsafe bool FloodFillPoint(byte* p, int stride, Point origin, int imageW, int imageH, Color startColor, int tolerance,
            Color fillColor, bool[,] doneChecking, Queue<Point> nextPoints, ref bool[,] floodFilled, bool fakeFill = false)
        {
            int ind = origin.Y * stride + origin.X * 4; // TODO: make sure all index operations multiply by 4 and not 3!

            if (!doneChecking[origin.X, origin.Y] && GetDifference(startColor, p, ind) <= tolerance)
            {
                // Mark this pixel as checked
                doneChecking[origin.X, origin.Y] = true;

                // Fill the color in
                if (!fakeFill)
                {
                    p[ind + 0] = fillColor.B;
                    p[ind + 1] = fillColor.G;
                    p[ind + 2] = fillColor.R;

                    floodFilled[origin.X, origin.Y] = true;
                }

                // Queue up the neighboring 4 pixels
                nextPoints.Enqueue(new Point((origin.X + 1) % imageW, origin.Y));
                nextPoints.Enqueue(new Point((origin.X - 1 + imageW) % imageW, origin.Y));
                nextPoints.Enqueue(new Point(origin.X, (origin.Y + 1) % imageH));
                nextPoints.Enqueue(new Point(origin.X, (origin.Y - 1 + imageH) % imageH));

                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the difference between two pixels in an unsafe context
        /// </summary>
        private unsafe int GetDifference(Color c, byte* b, int index)
        {
            return (int)Math.Max(Math.Max(Math.Abs(b[index + 0] - c.B), Math.Abs(b[index + 1] - c.G)), Math.Abs(b[index + 2] - c.R));
        }

        ///// <summary>
        ///// Remove blobs (fill with the background color) from an image under certain constraints.
        ///// </summary>
        ///// <param name="minimumBlobSize">The smallest number of pixels a blob can be made of</param>
        ///// <param name="minimumBlobWidth">The smallest width a blob can be</param>
        ///// <param name="minimumBlobHeight">The smallest height a blob can be</param>
        ///// <param name="backgroundColor">The color to fill small blobs with</param>
        //public void RemoveSmallBlobs(int minimumBlobSize, int minimumBlobWidth, int minimumBlobHeight, Color backgroundColor)
        //{
        //    RemoveSmallBlobs(minimumBlobSize, minimumBlobWidth, minimumBlobHeight, backgroundColor, 2);
        //}

        ///// <summary>
        ///// Remove blobs (fill with the background color) from an image under certain constraints.
        ///// </summary>
        ///// <param name="minimumBlobSize">The smallest number of pixels a blob can be made of</param>
        ///// <param name="minimumBlobWidth">The smallest width a blob can be</param>
        ///// <param name="minimumBlobHeight">The smallest height a blob can be</param>
        ///// <param name="backgroundColor">The color to fill small blobs with</param>
        ///// <param name="colorTolerance">The RGB tolerance in color when flood filling</param>
        //public void RemoveSmallBlobs(int minimumBlobSize, int minimumBlobWidth, int minimumBlobHeight, Color backgroundColor, int colorTolerance)
        //{
        //    try
        //    {
        //        // This will prevent us from attempting to count a blob of N pixels N times (assuming N < minimumBlobSize, otherwise it would be filled)
        //        bool[,] done = new bool[Image.Width, Image.Height];

        //        for (int x = 0; x < Image.Width; x++)
        //        {
        //            for (int y = 0; y < Image.Height; y++)
        //            {
        //                // Ignore the background
        //                if (!done[x, y] && Image.GetPixel(x, y).Subtract(backgroundColor) >= colorTolerance)
        //                {
        //                    // See how big of a blob there is here
        //                    BlobCount blob = FloodCount(new Point(x, y), colorTolerance, doneChecking: done);

        //                    // If it's small enough, fill it with the background color
        //                    if (blob.PixelCount < minimumBlobSize || blob.BlobBounds.Width < minimumBlobWidth || blob.BlobBounds.Height < minimumBlobHeight)
        //                    {
        //                        FloodFill(new Point(x, y), colorTolerance, backgroundColor);
        //                        // DEBUG: Color.FromArgb(Math.Min(255, blob.PixelCount), Math.Min(255, blob.PixelCount), Math.Min(255, blob.PixelCount)));
        //                    }
        //                }
        //            }
        //        }

        //        GlobalMessage.SendMessage(Image);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new ImageProcessingException("Error trying to remove small blobs from the image.", ex);
        //    }
        //}

        /// <summary>
        /// Fill each unique blob in an image with a random color.
        /// A group of adjacent pixels is considered a single blob when they are all similar to each other in the L*a*b* color space below a given threshold.
        /// In the L*a*b* color space, a threshold of 2.3 is considered to be a change "just noticible to the human eye."
        /// </summary>
        /// <param name="tolerance">The Delta E difference between two (L*a*b*) colors to allow when filling a blob.</param>
        /// <param name="background">The color of the background</param>
        /// <param name="backgroundTolerance">The Delta E difference between a pixel (L*a*b*) and the background to allow when filling.</param>
        public void ColorFillBlobs(double tolerance, Color background, double backgroundTolerance)
        {
            try
            {
                byte[,][] colors2 = new byte[Image.Width, Image.Height][];

                BitmapData bmData = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                int stride = bmData.Stride;
                IntPtr Scan0 = bmData.Scan0;

                bool[,] alreadyFilled = new bool[Image.Width, Image.Height];

                unsafe
                {
                    byte* p = (byte*)(void*)Scan0;
                    int nOffset = stride - Image.Width * 3;

                    for (int y = 0; y < Image.Height; ++y)
                    {
                        for (int x = 0; x < Image.Width; ++x)
                        {
                            // Store in BGR order
                            colors2[x, y] = new byte[] { p[0], p[1], p[2] };
                            p += 3;
                        }
                        p += nOffset;
                    }
                }

                Image.UnlockBits(bmData);

                int similarNeighborPixels;
                int pixelRadius = 1;

                for (int x = pixelRadius; x < Image.Width - pixelRadius; x++)
                {
                    for (int y = pixelRadius; y < Image.Height - pixelRadius; y++)
                    {
                        if (!alreadyFilled[x, y])
                        {
                            if (colors2[x, y].GetEDeltaColorDifference(background) > backgroundTolerance)
                            {
                                similarNeighborPixels = 0;
                                for (int xv = -pixelRadius; xv <= pixelRadius; xv++)
                                {
                                    for (int yv = -pixelRadius; yv <= pixelRadius; yv++)
                                    {
                                        if (yv != 0 || xv != 0)
                                        {
                                            if (colors2[x, y].GetEDeltaColorDifference(colors2[x + xv, y + yv]) < tolerance)
                                            {
                                                similarNeighborPixels++;
                                            }
                                        }
                                    }
                                }

                                if (similarNeighborPixels >= ((pixelRadius * 2 + 1) * (pixelRadius * 2 + 1)) - 1)
                                {
                                    FloodFill(new Point(x, y), (int)tolerance, ref alreadyFilled);
                                }
                            }
                            else
                            {
                                Image.SetPixel(x, y, background);
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Error trying to fill blobs in an image with random colors.", ex);
            }
        }

        //private Color GetMostCommonColor(double tolerance, double bkgTol)
        //{
        //    List<ColorCount> colors = new List<ColorCount>();
        //    ColorCount background = new ColorCount() { R = 255, G = 255, B = 255 };

        //    BitmapData bmData = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
        //    int stride = bmData.Stride;
        //    IntPtr Scan0 = bmData.Scan0;

        //    bool[,] alreadyFilled = new bool[Image.Width, Image.Height];

        //    unsafe
        //    {
        //        byte* p = (byte*)(void*)Scan0;
        //        int nOffset = stride - Image.Width * 3;

        //        for (int y = 0; y < Image.Height; ++y)
        //        {
        //            for (int x = 0; x < Image.Width; ++x)
        //            {
        //                ColorCount temp = new ColorCount() { R = p[2], G = p[1], B = p[0] };

        //                if (background.GetLABDist(temp) > bkgTol)
        //                {
        //                    int bestindex = -1;
        //                    double best = Double.MaxValue;
        //                    for (int i = 0; i < colors.Count; i++)
        //                    {
        //                        double test = colors[i].GetLABDist(temp);
        //                        if (test < best && test < tolerance)
        //                        {
        //                            best = test;
        //                            bestindex = i;
        //                        }
        //                    }
        //                    if (bestindex != -1 && colors.Count > 0)
        //                    {
        //                        colors[bestindex].FoundOne();
        //                    }
        //                    else
        //                    {
        //                        colors.Add(temp);
        //                    }
        //                }

        //                p += 3;
        //            }
        //            p += nOffset;
        //        }
        //    }

        //    Image.UnlockBits(bmData);

        //    colors.Sort((c, n) => -c.Count.CompareTo(n.Count));
        //    return colors.FirstOrDefault().Color;
        //}

        //public void KeepOnlyMostCommonColor(double tolerance)
        //{
        //    try
        //    {
        //        Color mostCommon = GetMostCommonColor(tolerance, tolerance * 1.5);

        //        BitmapData bmData = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
        //        int stride = bmData.Stride;
        //        IntPtr Scan0 = bmData.Scan0;

        //        unsafe
        //        {
        //            byte* p = (byte*)(void*)Scan0;
        //            int nOffset = stride - Image.Width * 3;

        //            for (int y = 0; y < Image.Height; ++y)
        //            {
        //                for (int x = 0; x < Image.Width; ++x)
        //                {
        //                    if (mostCommon.GetEDeltaColorDifference(p[2], p[1], p[0]) > tolerance)
        //                    {
        //                        p[2] = 255;
        //                        p[1] = 255;
        //                        p[0] = 255;
        //                    }
        //                    p += 3;
        //                }
        //                p += nOffset;
        //            }
        //        }

        //        Image.UnlockBits(bmData);

        //        GlobalMessage.SendMessage(Image);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new ImageProcessingException("Error keeping the most common color of an image.", ex); 
        //    }
        //}
    }

    public class ColorCount
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        private byte[] array { get { return new byte[] { R, G, B }; } }
        public int Count { get; set; }
        public Color Color { get { return Color.FromArgb(R, G, B); } }

        //public double GetLABDist(ColorCount other)
        //{
        //    return array.GetEDeltaColorDifference(other.array);
        //}

        public void FoundOne()
        {
            Count++;
        }

        public new string ToString()
        {
            return Count.ToString();
        }
    }

    public static class ImageExtensions2
    {

        /// <summary>
        /// A color difference algorithm to get the difference in visible color, and not just the integer difference in RGB values.
        /// NOTE: The JND (Just Noticible Difference) between two colors is about 2.3.
        /// </summary>
        public static double GetEDeltaColorDifference(this byte[] c, Color color)
        {
            // Yep. Very inefficient to create a Color here. TODO: Fix this.
            return GetEDeltaColorDifference(Color.FromArgb(c[2], c[1], c[0]), color);
        }

        /// <summary>
        /// A color difference algorithm to get the difference in visible color, and not just the integer difference in RGB values.
        /// NOTE: The JND (Just Noticible Difference) between two colors is about 2.3.
        /// </summary>
        public static double GetEDeltaColorDifference(this Color color, byte r, byte g, byte b)
        {
            return GetEDeltaColorDifference(Color.FromArgb(r, g, b), color);
        }

        /// <summary>
        /// A color difference algorithm to get the difference in visible color, and not just the integer difference in RGB values.
        /// NOTE: The JND (Just Noticible Difference) between two colors is about 2.3.
        /// </summary>
        public static double GetEDeltaColorDifference(this byte[] c, byte[] other)
        {
            // Yep. Very inefficient to create a Color here. TODO: Fix this.
            return GetEDeltaColorDifference(Color.FromArgb(c[2], c[1], c[0]), Color.FromArgb(other[2], other[1], other[0]));
        }

        /// <summary>
        /// A color difference algorithm to get the difference in visible color, and not just the integer difference in RGB values.
        /// NOTE: The JND (Just Noticible Difference) between two colors is about 2.3.
        /// </summary>
        public static double GetEDeltaColorDifference(this Color c, Color color)
        {
            LAB a = c.GetLAB();
            LAB b = color.GetLAB();

            return Math.Sqrt(Math.Pow(a.L - b.L, 2) + Math.Pow(a.a - b.a, 2) + Math.Pow(a.b - b.b, 2));
        }

        public static LAB GetLAB(this Color c)
        {
            return c.GetXYZ().GetLAB();
        }

        public static XYZ GetXYZ(this Color c)
        {
            // Adapted from http://www.easyrgb.com/index.php?X=MATH&H=07#text7

            double var_R = (c.R / 255.0);
            double var_G = (c.G / 255.0);
            double var_B = (c.B / 255.0);

            if (var_R > 0.04045) var_R = Math.Pow(((var_R + 0.055) / 1.055), 2.4);
            else var_R = var_R / 12.92;
            if (var_G > 0.04045) var_G = Math.Pow(((var_G + 0.055) / 1.055), 2.4);
            else var_G = var_G / 12.92;
            if (var_B > 0.04045) var_B = Math.Pow(((var_B + 0.055) / 1.055), 2.4);
            else var_B = var_B / 12.92;

            var_R = var_R * 100;
            var_G = var_G * 100;
            var_B = var_B * 100;

            XYZ xyz = new XYZ();
            xyz.X = var_R * 0.4124 + var_G * 0.3576 + var_B * 0.1805;
            xyz.Y = var_R * 0.2126 + var_G * 0.7152 + var_B * 0.0722;
            xyz.Z = var_R * 0.0193 + var_G * 0.1192 + var_B * 0.9505;

            return xyz;
        }

        public static LAB GetLAB(this XYZ c)
        {
            // Adapted from http://www.easyrgb.com/index.php?X=MATH&H=07#text7

            double var_X = c.X / 95.047;
            double var_Y = c.Y / 100.000;
            double var_Z = c.Z / 108.883;

            if (var_X > 0.008856) var_X = Math.Pow(var_X, (1.0 / 3));
            else var_X = (7.787 * var_X) + (16.0 / 116);
            if (var_Y > 0.008856) var_Y = Math.Pow(var_Y, (1.0 / 3));
            else var_Y = (7.787 * var_Y) + (16.0 / 116);
            if (var_Z > 0.008856) var_Z = Math.Pow(var_Z, (1.0 / 3));
            else var_Z = (7.787 * var_Z) + (16.0 / 116);

            LAB lab = new LAB();
            lab.L = (116 * var_Y) - 16;
            lab.a = 500 * (var_X - var_Y);
            lab.b = 200 * (var_Y - var_Z);

            return lab;
        }
    }

    public struct LAB
    {
        public double L { get; set; }
        public double a { get; set; }
        public double b { get; set; }
    }
    
    public struct XYZ
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }
}
