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
        public const int WhiteMinBrightness = 128;
        public const int WhiteMaxVariance = 64;
        public const double WhiteLabDeltaE = 7.5;

        // Settings for determining whether a rectangle is too small to be considered a card
        public const int MinDimension = 40;
        public const int MinArea = MinDimension * MinDimension;

        // Settings for overall image size
        public const int MaxImageDimention = 600;

        // Settings for card sizes
        public const int CardWidth = 100;
        public const int CardHeight = 60;
    }
}
