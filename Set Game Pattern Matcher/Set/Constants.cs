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
        public const int WhiteMaxVariance = 20;

        // Settings for determining whether a rectangle is too small to be considered a card
        public const int minDimension = 40;
        public const int minArea = minDimension * minDimension;

        // Settings for card sizes
        public const int cardWidth = 100;
        public const int cardHeight = 60;
    }
}
