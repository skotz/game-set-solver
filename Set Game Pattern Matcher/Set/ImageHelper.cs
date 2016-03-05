using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
