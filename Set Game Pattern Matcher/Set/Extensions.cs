using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Set_Game_Pattern_Matcher
{
    public static class Extensions
    {
        public static int Area(this Rectangle r)
        {
            return r.Width * r.Height;
        }

        public static bool IsWhite(this Bitmap b, int x, int y)
        {
            return IsWhite(b.GetPixel(x, y));
        }

        public static bool IsWhite(this Color c)
        {
            return c.R >= Constants.threshold && c.G >= Constants.threshold && c.B > Constants.threshold &&
                Math.Abs(c.R - c.G) < Constants.variance && Math.Abs(c.G - c.B) < Constants.variance && Math.Abs(c.B - c.R) < Constants.variance;
        }
    }
}
