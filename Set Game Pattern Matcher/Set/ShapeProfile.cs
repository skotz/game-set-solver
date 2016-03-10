using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Set_Game_Pattern_Matcher
{
    class ShapeProfile
    {
        public List<double> Profile { get; private set; }

        public CardShape Shape { get; set; }

        public ShapeProfile(List<double> profile, CardShape shape)
        {
            Profile = profile;
            Shape = shape;
            Normalize();
        }

        public ShapeProfile(string profile, CardShape shape)
        {
            Profile = profile.Split(',').Select(x => double.Parse(x)).ToList();
            Shape = shape;
            Normalize();
        }

        public ShapeProfile(List<double> profile)
        {
            Profile = profile;
            Normalize();
        }

        private void Normalize()
        {
            double min = Profile.Min();
            double diff = Profile.Max() - min;
            Profile = Profile.Select(x => (x - min) / diff).ToList();
        }

        public double CompareTo(ShapeProfile other)
        {
            double sum = 0;
            for (int i = 0; i < Profile.Count; i++)
            {
                sum += Math.Pow(Profile[i] - other.Profile[i], 2);
            }
            return Math.Sqrt(sum / Profile.Count);
        }

        public static CardShape GetShape(List<ShapeProfile> profiles, List<double> profile)
        {
            ShapeProfile p = new ShapeProfile(profile);
            double best = double.MaxValue;
            CardShape bestShape = profiles[0].Shape;

            foreach (ShapeProfile sp in profiles)
            {
                double test = sp.CompareTo(p);
                if (test < best)
                {
                    best = test;
                    bestShape = sp.Shape;
                }
            }

            return bestShape;
        }
    }
}
