using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Set_Game_Pattern_Matcher
{
    class SetCard
    {
        public CardColor Color { get; set; }
        public CardShading Shade { get; set; }
        public CardShape Shape { get; set; }
        public int Number { get; set; }

        public Rectangle OriginalRectangle { get; set; }

        public SetCard(int number, CardShape shape, CardShading shade, CardColor color)
            : this(number, shape, shade, color, new Rectangle(0, 0, 1, 1))
        {
        }

        public SetCard(int number, CardShape shape, CardShading shade, CardColor color, Rectangle originalRect)
        {
            Color = color;
            Shade = shade;
            Shape = shape;
            Number = number;
            OriginalRectangle = originalRect;
        }
    }
}
