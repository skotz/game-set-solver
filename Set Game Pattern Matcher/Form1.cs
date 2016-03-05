using ScottClayton.Image;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Set_Game_Pattern_Matcher
{
    public partial class Form1 : Form
    {
        // Settings for determining whether a color is "white-ish"
        int threshold = 128;
        int variance = 64;

        // Settings for determining whether a rectangle is too small to be considered a card
        int minDimension = 10;
        int minArea = 100;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Bitmap b = (Bitmap)Bitmap.FromFile("../../../Test Images/Google Images/setcards.jpg");

            FindSets(b);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Bitmap b = (Bitmap)Bitmap.FromFile("../../../Test Images/Google Images/set2.jpg");

            FindSets(b);
        }

        private int GetCardShape(Bitmap img)
        {
            BlobSegmentMethod s = new BlobSegmentMethod(10, 30, 3);

            Segmenter seg = new Segmenter() { Image = img };
            seg.FloodFill(new Point(2, 2), 32, Color.White);
            seg.ColorFillBlobs(80, Color.White, 32);

            return s.Segment(img, variance).Count;
        }

        private int GetCardNumber(Bitmap img, CardColor color)
        {
            List<int> histogram = new List<int>();

            int sum;
            for (int x = 0; x < img.Width; x++)
            {
                sum = 0;
                for (int y = 0; y < img.Height; y++)
                {
                    Color c = img.GetPixel(x, y);
                    if (!IsWhite(c))
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
            for (int i=0; i<histogram.Count; i++)
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

        private CardColor GetCardColor(Bitmap img)
        {
            long r = 0;
            long g = 0;
            long b = 0;

            for (int x = 0; x < img.Width; x++)
            {
                for (int y = 0; y < img.Height; y++)
                {
                    Color c = img.GetPixel(x, y);
                    if (!IsWhite(c))
                    {
                        r += c.R;
                        g += c.G;
                        b += c.B;
                    }
                    //else
                    //{
                    //    img.SetPixel(x, y, Color.White);
                    //}
                }
            }

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

        private void FindSets(Bitmap b)
        {
            Rectangle temp;
            List<Rectangle> sets = new List<Rectangle>();

            for (int x = 0; x < b.Width; x++)
            {
                for (int y = 0; y < b.Height; y++)
                {
                    if (IsWhite(b, x, y))
                    {
                        temp = TraceSet(b, x, y);

                        if (temp.Area() >= minArea && temp.Width >= minDimension && temp.Height > minDimension)
                        {
                            sets.Add(temp);
                        }
                    }
                }
            }

            RemoveOverlaps(sets);
            KeepTopSets(sets, (int)numericUpDown1.Value);

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
                        switch (color)
                        {
                            case CardColor.Red:
                                gc.DrawString("Red " + count, new Font("Consolas", 12.0f), Brushes.Red, new Point(1, 1));
                                break;
                            case CardColor.Green:
                                gc.DrawString("Green " + count, new Font("Consolas", 12.0f), Brushes.Green, new Point(1, 1));
                                break;
                            case CardColor.Blue:
                                gc.DrawString("Blue " + count, new Font("Consolas", 12.0f), Brushes.Blue, new Point(1, 1));
                                break;
                        }

                        card.Save("card-" + (cardNo++) + ".png", ImageFormat.Png);
                    }

                    g.DrawRectangle(Pens.Red, r);
                }
            }

            b.Save("patterns.png", ImageFormat.Png);
            pictureBox1.Image = b;
        }

        private void KeepTopSets(List<Rectangle> sets, int max)
        {
            if (sets.Count > max)
            {
                sets.Sort((c, n) => n.Area().CompareTo(c.Area()));
                sets.RemoveRange(max, sets.Count - max);
            }
        }

        private void RemoveOverlaps(List<Rectangle> sets)
        {
            //sets.RemoveAll(x => x.Width * x.Height < 100 * 100);

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

        private Rectangle TraceSet(Bitmap b, int startx, int starty)
        {
            int right = b.Width;
            for (int x = startx; x < right; x++)
            {
                if (!IsWhite(b, x, starty))
                {
                    right = x;
                }
            }

            int bottom = b.Height;
            for (int y = starty; y < bottom; y++)
            {
                if (!IsWhite(b, startx, y))
                {
                    bottom = y;
                }
            }

            // Move up-right diagonally until we hit a white square
            for (int i = 0; i < Math.Min(right - startx - 1, bottom - starty - 1); i++)
            {
                if (IsWhite(b, right - i, bottom - i))
                {
                    right -= i;
                    bottom -= i;
                    break;
                }
            }

            return new Rectangle(startx, starty, right - startx - 1, bottom - starty - 1);
        }

        private bool IsWhite(Bitmap b, int x, int y)
        {
            return IsWhite(b.GetPixel(x, y));
        }

        private bool IsWhite(Color c)
        {
            return c.R >= threshold && c.G >= threshold && c.B > threshold && Math.Abs(c.R - c.G) < variance && Math.Abs(c.G - c.B) < variance && Math.Abs(c.B - c.R) < variance;
        }
    }
    
    public enum CardColor
    {
        Red,
        Green,
        Blue
    }

    public static class Extensions
    {
        public static int Area(this Rectangle r)
        {
            return r.Width * r.Height;
        }
    }
}
