using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Set_Game_Pattern_Matcher
{
    class SetCardMatch
    {
        public List<SetCard> Cards { get; private set; }

        public Bitmap Image { get; set; }

        public SetCardMatch()
        {
            Cards = new List<SetCard>();
        }

        public void Add(SetCard card)
        {
            Cards.Add(card);
        }

        public bool IsMatch()
        {
            if (Cards.Count == 3)
            {
                // Color: all the same or all different
                bool color = (Cards[0].Color == Cards[1].Color && Cards[1].Color == Cards[2].Color) ||
                    (Cards[0].Color != Cards[1].Color && Cards[1].Color != Cards[2].Color && Cards[2].Color != Cards[0].Color);

                // Number: all the same or all different
                bool number = (Cards[0].Number == Cards[1].Number && Cards[1].Number == Cards[2].Number) ||
                    (Cards[0].Number != Cards[1].Number && Cards[1].Number != Cards[2].Number && Cards[2].Number != Cards[0].Number);

                // Shade: all the same or all different
                bool shade = (Cards[0].Shade == Cards[1].Shade && Cards[1].Shade == Cards[2].Shade) ||
                    (Cards[0].Shade != Cards[1].Shade && Cards[1].Shade != Cards[2].Shade && Cards[2].Shade != Cards[0].Shade);

                // Shape: all the same or all different
                bool shape = (Cards[0].Shape == Cards[1].Shape && Cards[1].Shape == Cards[2].Shape) ||
                    (Cards[0].Shape != Cards[1].Shape && Cards[1].Shape != Cards[2].Shape && Cards[2].Shape != Cards[0].Shape);

                return color && number && shape && shade;
            }

            return false;
        }

        public static List<SetCardMatch> GetMatches(List<SetCard> allCards)
        {
            List<SetCardMatch> allMatches = new List<SetCardMatch>();

            for (int a = 0; a < allCards.Count; a++)
            {
                for (int b = a + 1; b < allCards.Count; b++)
                {
                    for (int c = b + 1; c < allCards.Count; c++)
                    {
                        SetCardMatch set = new SetCardMatch();
                        set.Add(allCards[a]);
                        set.Add(allCards[b]);
                        set.Add(allCards[c]);

                        if (set.IsMatch())
                        {
                            allMatches.Add(set);
                        }
                    }
                }
            }

            return allMatches;
        }
    }
}
