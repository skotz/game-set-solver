using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Set_Game_Pattern_Matcher
{
    class SetGame
    {
        public event EventHandler<Bitmap> OnDebugImage;

        private List<Pattern> shapePatterns;

        int test = 1;

        public SetGame()
        {
            shapePatterns = Pattern.LoadPrimaryPatterns();
        }

        /// <summary>
        /// Get the shape of a card
        /// </summary>
        /// <param name="img">The image to determine the shape from</param>
        /// <returns></returns>
        private CardShape GetCardShape(Bitmap img)
        {
            List<int> histogram = new List<int>();

            BitmapData bmData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bmData.Stride;

            unsafe
            {
                byte* p = (byte*)(void*)bmData.Scan0;

                // Create a histogram that represents the number of white pixels from the left border on each line
                for (int y = 0; y < img.Height; y++)
                {
                    for (int x = 0; x < img.Width; x++)
                    {
                        int i = ImageHelper.GetBmpDataIndex(x, y, stride);

                        if (!ImageHelper.IsWhite(p, i))
                        {
                            histogram.Add(x);
                            break;
                        }
                    }
                }
            }

            img.UnlockBits(bmData);

            // Group the histogram values into a buckets of averages
            List<double> averages = new List<double>();
            int sections = 4;
            for (int i = 0; i < sections; i++)
            {
                double sum = 0;
                int actual = 0;
                for (int x = i * (histogram.Count / sections); x <= (i + 1) * (histogram.Count / (double)sections) && x < histogram.Count; x++)
                {
                    sum += histogram[x];
                    actual++;
                }
                sum /= actual;
                averages.Add(sum);
            }

            // Based on the data analysis of the histograms, these rules work quite well in determining the shape
            if (averages[0] < averages[1] && averages[1] > averages[2] && averages[2] < averages[3])
            {
                return CardShape.Squiggle;
            }
            else if (averages[0] > averages[1] + 4 && averages[3] > averages[2] + 4)
            {
                return CardShape.Diamond;
            }
            else
            {
                return CardShape.Oval;
            }

            // File.WriteAllText("card-" + (test++) + ".txt", blah + "\r\n" + averages.Select(x => x.ToString("0.00")).Aggregate((c, n) => c + "\r\n" + n));
        }

        /// <summary>
        /// Gets the number of a card
        /// </summary>
        /// <param name="img">The image to determine the number from</param>
        /// <param name="fromRight">Whether to search starting from the left (true) or the right (false)</param>
        /// <returns></returns>
        private int GetCardNumber(Bitmap img, bool fromRight = false)
        {
            List<int> histogram = new List<int>();

            BitmapData bmData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bmData.Stride;

            unsafe
            {
                byte* p = (byte*)(void*)bmData.Scan0;

                // Create a histogram that represents the number of white pixels from the left border on each line
                for (int y = 0; y < img.Height; y++)
                {
                    for (int x = 0; x < img.Width / 2; x++)
                    {
                        int effectiveX = fromRight ? img.Width - 1 - x : x;
                        int i = ImageHelper.GetBmpDataIndex(effectiveX, y, stride);

                        if (!ImageHelper.IsWhite(p, i))
                        {
                            //p[i + 1] = 255;
                            histogram.Add(x);
                            break;
                        }
                        //else
                        //{
                        //    p[i + 2] = 255;
                        //}
                    }
                }
            }

            img.UnlockBits(bmData);

            double average = histogram.Count > 0 ? histogram.Average() : 0;

            if (average > (Constants.cardWidth / 2) - 3 || histogram.Count < 5)
            {
                // Since our average indicates that nearly every pixel in the left half of the image is white, count again starting from the right
                if (!fromRight)
                {
                    return GetCardNumber(img, true);
                }
            }
            
            // Based on the analysis of existing patterns
            if (average > 35.0)
            {
                return 1;
            }
            else if (average > 15.0)
            {
                return 2;
            }
            else
            {
                return 3;
            }

            // File.WriteAllText("card-" + (test++) + ".txt", blah + "\r\n" + average.ToString());
        }

        /// <summary>
        /// Get the color of a card
        /// </summary>
        /// <param name="img">The image to determine the color from</param>
        /// <returns></returns>
        private CardColor GetCardColor(Bitmap img)
        {
            long r = 0;
            long g = 0;
            long b = 0;

            BitmapData bmData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bmData.Stride;

            unsafe
            {
                byte* p = (byte*)(void*)bmData.Scan0;

                for (int x = 0; x < img.Width; x++)
                {
                    for (int y = 0; y < img.Height; y++)
                    {
                        int i = ImageHelper.GetBmpDataIndex(x, y, stride);

                        if (!ImageHelper.IsWhite(p, i))
                        {
                            r += p[i + 2];
                            g += p[i + 1];
                            b += p[i];
                        }
                    }
                }
            }

            img.UnlockBits(bmData);

            if (r > g && r > b)
            {
                return CardColor.Red;
            }
            else if (g > r && g > b)
            {
                return CardColor.Green;
            }
            else
            {
                return CardColor.Blue;
            }
        }

        /// <summary>
        /// Get the shading of a card
        /// </summary>
        /// <param name="img">The image to determine the shading from</param>
        /// <returns></returns>
        private CardShading GetCardShading(Bitmap img, CardColor color)
        {
            int lastShade = 0;
            bool lastWhite = true;
            int jumpsToWhite = 0;
            int jumpsToLighterColor = 0;
            int lastX = 0;
            List<int> lineThickness = new List<int>();

            BitmapData bmData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bmData.Stride;

            unsafe
            {
                byte* p = (byte*)(void*)bmData.Scan0;

                for (int y = 0; y < img.Height; y++, lastX = 0)
                {
                    for (int x = 0; x < img.Width; x++)
                    {
                        int i = ImageHelper.GetBmpDataIndex(x, y, stride);

                        int brightness;
                        switch (color)
                        {
                            case CardColor.Red:
                                brightness = p[i + 2];
                                break;
                            case CardColor.Green:
                                brightness = p[i + 1];
                                break;
                            case CardColor.Blue:
                            default:
                                brightness = p[i];
                                break;
                        }

                        bool isWhite = ImageHelper.IsWhite(p, i);

                        if (Math.Abs(lastShade - brightness) > 5)
                        {
                            if (isWhite)
                            {
                                jumpsToWhite++;

                                if (lastX != 0)
                                {
                                    lineThickness.Add(x - lastX);
                                    lastX = 0;
                                }

                                //p[i + 2] = 255;
                                //p[i + 1] = 0;
                                //p[i] = 0;
                            }
                            else if (brightness > lastShade && !lastWhite)
                            {
                                jumpsToLighterColor++;

                                if (lastX != 0)
                                {
                                    lineThickness.Add(x - lastX);
                                    lastX = 0;
                                }

                                //p[i + 2] = 0;
                                //p[i + 1] = 255;
                                //p[i] = 0;
                            }

                            if (!isWhite && lastWhite)
                            {
                                // We hit a line
                                lastX = x;
                                
                                //p[i + 2] = 0;
                                //p[i + 1] = 0;
                                //p[i] = 255;
                            }
                        }
                        //else
                        //{
                        //    p[i + 2] = 255;
                        //    p[i + 1] = 255;
                        //    p[i] = 255;
                        //}

                        lastShade = brightness;
                        lastWhite = isWhite;
                    }
                }
            }

            img.UnlockBits(bmData);

            lineThickness.RemoveAll(x => x == 0);
            double average = lineThickness.Count > 0 ? lineThickness.Average() : 0;

            //File.WriteAllText("card-" + (test++) + ".txt", jumpsToWhite + "\r\n" + jumpsToLighterColor + "\r\n" + average + "\r\n-----\r\n" + lineThickness.Select(x => x.ToString()).Aggregate((c, n) => c + "\r\n" + n));

            if (average > 8)
            {
                return CardShading.Filled;
            }
            if (jumpsToLighterColor > jumpsToWhite)
            {
                return CardShading.Shaded;
            }
            else
            {
                return CardShading.Empty;
            }
        }

        public void FindSets(Bitmap b, int numberOfSets = 12)
        {
            b = ImageHelper.Resize(b, 600);

            // b.Save("resized.png", ImageFormat.Png);

            Rectangle temp;
            List<Rectangle> sets = new List<Rectangle>();

            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bmData.Stride;

            unsafe
            {
                byte* p = (byte*)(void*)bmData.Scan0;
                int nOffset = stride - b.Width * 3;

                for (int x = 0; x < b.Width; x++)
                {
                    for (int y = 0; y < b.Height; y++)
                    {
                        if (ImageHelper.IsWhite(p, x, y, stride))
                        {
                            temp = TraceSet(p, x, y, b.Width, b.Height, stride);

                            if (temp.Area() >= Constants.minArea && temp.Width >= Constants.minDimension && temp.Height > Constants.minDimension)
                            {
                                sets.Add(temp);
                            }

                            //int i = ImageHelper.GetBmpDataIndex(x, y, stride);
                            //p[i + 2] = 255;
                            //p[i + 1] = 255;
                            //p[i] = 255;
                        }
                    }
                }
            }

            b.UnlockBits(bmData);

            RemoveOverlaps(sets);
            KeepTopSets(sets, numberOfSets);

            int cardNo = 1;

            using (Graphics g = Graphics.FromImage(b))
            {
                foreach (Rectangle r in sets)
                {
                    bool tall = r.Height > r.Width;
                    using (Bitmap card = new Bitmap(tall ? Constants.cardHeight : Constants.cardWidth, tall ? Constants.cardWidth : Constants.cardHeight))
                    using (Graphics gc = Graphics.FromImage(card))
                    {
                        gc.DrawImage(b, new Rectangle(0, 0, card.Width, card.Height), r, GraphicsUnit.Pixel);
                        if (tall)
                        {
                            card.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        }

                        CardColor color = GetCardColor(card);
                        int count = GetCardNumber(card);
                        CardShape shape = GetCardShape(card);
                        CardShading shade = GetCardShading(card, color);
                        
                        gc.DrawString(color.ToString()[0] + count.ToString() + shape.ToString()[0] + shade.ToString()[0], new Font("Consolas", 12.0f), Brushes.Black, new Point(1, 1));

                        card.Save("card-" + (cardNo++) + ".png", ImageFormat.Png);
                    }

                    g.DrawRectangle(Pens.Red, r);
                }
            }

            b.Save("patterns.png", ImageFormat.Png);

            if (OnDebugImage != null)
            {
                OnDebugImage(this, b);
            }
        }

        /// <summary>
        /// Delete all but the top N largest rectangles.
        /// </summary>
        /// <param name="sets">The list of rectanges to search</param>
        /// <param name="max">The number of rectangles to keep</param>
        private void KeepTopSets(List<Rectangle> sets, int max)
        {
            if (sets.Count > max)
            {
                sets.Sort((c, n) => n.Area().CompareTo(c.Area()));
                sets.RemoveRange(max, sets.Count - max);
            }
        }

        /// <summary>
        /// Find all rectanges that intersect with another rectangle and delete the smaller of the two.
        /// </summary>
        /// <param name="sets">The list of rectanges to search</param>
        private void RemoveOverlaps(List<Rectangle> sets)
        {
            for (int i = sets.Count - 1; i >= 0; i--)
            {
                for (int r = sets.Count - 1; r >= 0 && i < sets.Count; r--)
                {
                    // When two rectangles intersect
                    if (Rectangle.Intersect(sets[i], sets[r]) != Rectangle.Empty && i != r)
                    {
                        // Take the larger of the two
                        if (sets[i].Width * sets[i].Height < sets[r].Width * sets[r].Height)
                        {
                            sets.RemoveAt(i);
                        }
                        else
                        {
                            sets.RemoveAt(r);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// From a given starting pixel, find the largest rectangle whose borders are entirely drawn over white pixels
        /// </summary>
        /// <param name="image">The image to search</param>
        /// <param name="startx">The X position to start searching from</param>
        /// <param name="starty">The Y position to start searching from</param>
        /// <param name="width">The width of the image</param>
        /// <param name="height">The height of the image</param>
        /// <param name="stride">The stride of the image data</param>
        /// <returns></returns>
        private unsafe Rectangle TraceSet(byte* image, int startx, int starty, int width, int height, int stride)
        {
            int right = width - 1;
            for (int x = startx; x < right; x++)
            {
                if (!ImageHelper.IsWhite(image, x, starty, stride))
                {
                    right = x;
                }
            }

            int bottom = height - 1;
            for (int y = starty; y < bottom; y++)
            {
                if (!ImageHelper.IsWhite(image, startx, y, stride))
                {
                    bottom = y;
                }
            }

            // Move up-left diagonally until we hit a white square
            for (int i = 0; i < Math.Min(right - startx - 1, bottom - starty - 1); i++)
            {
                if (ImageHelper.IsWhite(image, right - i, bottom - i, stride))
                {
                    right -= i;
                    bottom -= i;
                    break;
                }
            }

            return new Rectangle(startx, starty, right - startx - 1, bottom - starty - 1);
        }
    }
}
