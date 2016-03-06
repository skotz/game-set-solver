using ScottClayton.Image;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Set_Game_Pattern_Matcher
{
    class SetGame
    {
        public event EventHandler<Bitmap> OnDebugImage;

        private List<Pattern> shapePatterns;

        private int test = 1;

        public SetGame()
        {
            shapePatterns = Pattern.LoadPrimaryPatterns();
        }

        private CardShape GetCardShape(Bitmap img, int count)
        {
            Bitmap copy = ImageHelper.Copy(img);

            BlobSegmentMethod blob = new BlobSegmentMethod(10, 30, count);            
            Segmenter seg = new Segmenter() { Image = copy };
            seg.FloodFill(new Point(2, 2), 32, Color.White);
            seg.ColorFillBlobs(80, Color.White, 32);

            foreach (Bitmap pic in blob.Segment(copy, Constants.variance))
            {
                using (Bitmap pattern = new Bitmap(32, 64))
                using (Graphics gc = Graphics.FromImage(pattern))
                {
                    gc.DrawImage(pic, new Rectangle(0, 0, pattern.Width, pattern.Height), new Rectangle(0, 0, pic.Width, pic.Height), GraphicsUnit.Pixel);

                    pattern.Save("x-pic-" + (test++) + ".png", ImageFormat.Png);

                    Pattern p = new Pattern(pattern);
                    return p.GetClosestPattern(shapePatterns).Shape;
                }
            }

            return CardShape.Squiggle;
        }

        private int GetCardNumber(Bitmap img, CardColor color)
        {
            List<int> histogram = new List<int>();

            //Segmenter seg = new Segmenter() { Image = img };
            //seg.FloodFill(new Point(2, 2), 32, Color.White);
            //seg.ColorFillBlobs(80, Color.White, 32);
            //seg.ResizeRotateCut(true);

            //img.Save("hist.png", ImageFormat.Png);

            int sum;
            for (int x = 0; x < img.Width; x++)
            {
                sum = 0;
                for (int y = 0; y < img.Height; y++)
                {
                    Color c = img.GetPixel(x, y);
                    if (!c.IsWhite())
                    {
                        if (color == CardColor.Red)
                        {
                            sum += c.R;
                        }
                        else if (color == CardColor.Green)
                        {
                            sum += c.G;
                        }
                        else if (color == CardColor.Blue)
                        {
                            sum += c.B;
                        }
                    }
                }
                histogram.Add(sum);
            }

            bool lastZero = false;
            int sections = 0;
            for (int i = 0; i < histogram.Count; i++)
            {
                if (lastZero && histogram[i] != 0 && i + 1 < histogram.Count && histogram[i + 1] != 0 && i + 2 < histogram.Count && histogram[i + 2] != 0)
                {
                    lastZero = false;
                }
                else if (!lastZero && histogram[i] == 0 && i + 1 < histogram.Count && histogram[i + 1] == 0 && i + 2 < histogram.Count && histogram[i + 2] == 0)
                {
                    sections++;
                    lastZero = true;
                }
            }

            // File.WriteAllText("histogram.txt", histogram.Select(x => x.ToString()).Aggregate((c, n) => c + ", " + n));

            return sections - 1;
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

        public void FindSets(Bitmap b, int numberOfSets = 12)
        {
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
                    using (Bitmap card = new Bitmap(tall ? 100 : 150, tall ? 150 : 100))
                    using (Graphics gc = Graphics.FromImage(card))
                    {
                        gc.DrawImage(b, new Rectangle(0, 0, card.Width, card.Height), r, GraphicsUnit.Pixel);
                        if (tall)
                        {
                            card.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        }

                        CardColor color = GetCardColor(card);
                        int count = GetCardNumber(card, color);
                        CardShape shape = GetCardShape(card, count);
                        switch (color)
                        {
                            case CardColor.Red:
                                gc.DrawString(count + " " + shape.ToString(), new Font("Consolas", 12.0f), Brushes.Red, new Point(1, 1));
                                break;
                            case CardColor.Green:
                                gc.DrawString(count + " " + shape.ToString(), new Font("Consolas", 12.0f), Brushes.Green, new Point(1, 1));
                                break;
                            case CardColor.Blue:
                                gc.DrawString(count + " " + shape.ToString(), new Font("Consolas", 12.0f), Brushes.Blue, new Point(1, 1));
                                break;
                        }

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
