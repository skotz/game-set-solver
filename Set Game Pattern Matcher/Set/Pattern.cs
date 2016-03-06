using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Set_Game_Pattern_Matcher
{
    class Pattern
    {
        public Bitmap Image { get; set; }

        public CardShape Shape { get; set; }

        public Pattern(Bitmap image)
        {
            Image = image;
        }

        public Pattern(Bitmap image, CardShape shape)
        {
            Image = image;
            Shape = shape;
        }

        /// <summary>
        /// Calculate the root mean square distance between two patterns
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public double GetDifference(Pattern other)
        {
            if (Image.Width != other.Image.Width || Image.Height != other.Image.Height)
            {
                throw new Exception("Patterns must have the same dimentions!");
            }

            BitmapData bmpData = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            BitmapData otherBmpData = other.Image.LockBits(new Rectangle(0, 0, other.Image.Width, other.Image.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bmpData.Stride;

            double sum = 0.0;

            unsafe
            {
                byte* p1 = (byte*)(void*)bmpData.Scan0;
                byte* p2 = (byte*)(void*)otherBmpData.Scan0;

                for (int x = 0; x < Image.Width; x++)
                {
                    for (int y = 0; y < Image.Height; y++)
                    {
                        int i = ImageHelper.GetBmpDataIndex(x, y, stride);
                        sum += (p1[i] - p2[i]) * (p1[i] - p2[i]) + (p1[i + 1] - p2[i + 1]) * (p1[i + 1] - p2[i + 1]) + (p1[i + 2] - p2[i + 2]) * (p1[i + 2] - p2[i + 2]);
                    }
                }
            }

            Image.UnlockBits(bmpData);
            other.Image.UnlockBits(otherBmpData);

            return Math.Sqrt(sum / (other.Image.Width * other.Image.Height * 3));
        }

        public static List<Pattern> LoadPrimaryPatterns()
        {
            List<Pattern> patterns = new List<Pattern>();

            patterns.Add(new Pattern(ImageHelper.GetEmbeddedImage("diamond.png"), CardShape.Diamond));
            patterns.Add(new Pattern(ImageHelper.GetEmbeddedImage("oval.png"), CardShape.Oval));
            patterns.Add(new Pattern(ImageHelper.GetEmbeddedImage("squiggle.png"), CardShape.Squiggle));

            return patterns;
        }

        public Pattern GetClosestPattern(List<Pattern> patterns)
        {
            Pattern best = patterns[0];
            double bestVal = double.MaxValue;

            foreach (Pattern p in patterns)
            {
                double diff = GetDifference(p);
                if (diff < bestVal)
                {
                    bestVal = diff;
                    best = p;
                }
            }

            return best;
        }
    }
}
