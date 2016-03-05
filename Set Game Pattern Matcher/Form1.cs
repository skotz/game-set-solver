using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Set_Game_Pattern_Matcher
{
    public partial class Form1 : Form
    {
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

        private void FindSets(Bitmap b)
        {
            List<Rectangle> sets = new List<Rectangle>();

            for (int x = 0; x < b.Width; x++)
            {
                for (int y = 0; y < b.Height; y++)
                {
                    if (IsWhite(b, x, y))
                    {
                        sets.Add(TraceSet(b, x, y));
                    }
                }
            }

            WeedOutDuplicates(sets);

            using (Graphics g = Graphics.FromImage(b))
            {
                foreach (Rectangle r in sets)
                {
                    g.DrawRectangle(Pens.Red, r);
                }
            }

            pictureBox1.Image = b;
        }

        private void WeedOutDuplicates(List<Rectangle> sets)
        {
            //sets.RemoveAll(x => x.Width * x.Height < 100 * 100);

            for (int i = sets.Count - 1; i >= 0; i--)
            {
                //if (sets.Any(x => x.Contains(sets[i].X, sets[i].Y)))
                //{
                //    sets.RemoveAt(i);
                //}

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
            int threshold = 128;
            int variance = 64;
            return c.R >= threshold && c.G >= threshold && c.B > threshold && Math.Abs(c.R - c.G) < variance && Math.Abs(c.G - c.B) < variance && Math.Abs(c.B - c.R) < variance;
        }
    }
}
