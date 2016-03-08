using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Set_Game_Pattern_Matcher
{
    class Constants
    {
        // Settings for determining whether a color is "white-ish"
        public const int threshold = 128;
        public const int variance = 64;

        // Settings for determining whether a rectangle is too small to be considered a card
        public const int minDimension = 40;
        public const int minArea = minDimension * minDimension;
    }
}
