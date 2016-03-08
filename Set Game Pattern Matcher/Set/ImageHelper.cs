using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Set_Game_Pattern_Matcher
{
    class ImageHelper
    {
        public static unsafe bool IsWhite(byte* image, int startOffset)
        {
            return image[startOffset + 2] >= Constants.threshold && image[startOffset + 1] >= Constants.threshold && image[startOffset] > Constants.threshold &&
                Math.Abs(image[startOffset + 2] - image[startOffset + 1]) < Constants.variance && Math.Abs(image[startOffset + 1] - image[startOffset]) < Constants.variance && Math.Abs(image[startOffset] - image[startOffset + 2]) < Constants.variance;
        }

        public static unsafe bool IsWhite(byte* image, int x, int y, int stride)
        {
            return IsWhite(image, GetBmpDataIndex(x, y, stride));
        }

        public static int GetBmpDataIndex(int x, int y, int stride)
        {
            return y * stride + x * 3;
        }

        public static Bitmap Copy(Bitmap image)
        {
            Bitmap bmp = new Bitmap(image.Width, image.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.DrawImage(image, 0, 0, new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
            }
            return bmp;
        }

        public static Bitmap Resize(Bitmap image, int maxDimention)
        {
            int newWidth = Math.Min(image.Width, maxDimention);
            int newHeight = (int)((double)image.Height * ((double)newWidth / (double)image.Width));

            if (newHeight > maxDimention)
            {
                newHeight = maxDimention;
                newWidth = (int)((double)image.Width * ((double)newHeight / (double)image.Height));
            }

            Bitmap newImage = new Bitmap(newWidth, newHeight);
            using (Graphics gc = Graphics.FromImage(newImage))
            {
                gc.DrawImage(image, new Rectangle(0, 0, newImage.Width, newImage.Height), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
            }

            return newImage;
        }

        public static Bitmap GetEmbeddedImage(string name)
        {
            foreach (string resource in Assembly.GetEntryAssembly().GetManifestResourceNames())
            {
                if (resource.ToLower().EndsWith(name.ToLower()))
                {
                    return (Bitmap)Bitmap.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(resource));
                }
            }

            throw new Exception("Could not find embedded resource \"" + name + "\"");
        }
    }
}
