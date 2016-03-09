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
    }
}
