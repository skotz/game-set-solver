﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Set_Game_Pattern_Matcher
{
    public struct LABColor
    {
        public float L { get; set; }
        public float A { get; set; }
        public float B { get; set; }

        public LABColor(float l, float a, float b)
            : this()
        {
            L = l;
            A = a;
            B = b;
        }

        public LABColor(Color col)
            : this()
        {
            LABColor temp = FromColor(col);
            L = temp.L;
            A = temp.A;
            B = temp.B;
        }
        
        /// <summary>
        /// Get the Delta E color difference between two colors.
        /// </summary>
        public static double Distance(LABColor a, LABColor b)
        {
            return Math.Sqrt(Math.Pow(a.L - b.L, 2) + Math.Pow(a.A - b.A, 2) + Math.Pow(a.B - b.B, 2));
        }

        public static LABColor FromColor(Color c)
        {
            return FromColor(c.R, c.G, c.B);
        }

        public static LABColor FromColor(int red, int green, int blue)
        {
            float D65x = 0.9505f;
            float D65y = 1.0f;
            float D65z = 1.0890f;
            float rLinear = red / 256f;
            float gLinear = green / 256f;
            float bLinear = blue / 256f;
            float r = (rLinear > 0.04045f) ? (float)Math.Pow((rLinear + 0.055f) / (1f + 0.055f), 2.2f) : (rLinear / 12.92f);
            float g = (gLinear > 0.04045f) ? (float)Math.Pow((gLinear + 0.055f) / (1f + 0.055f), 2.2f) : (gLinear / 12.92f);
            float b = (bLinear > 0.04045f) ? (float)Math.Pow((bLinear + 0.055f) / (1f + 0.055f), 2.2f) : (bLinear / 12.92f);
            float x = (r * 0.4124f + g * 0.3576f + b * 0.1805f);
            float y = (r * 0.2126f + g * 0.7152f + b * 0.0722f);
            float z = (r * 0.0193f + g * 0.1192f + b * 0.9505f);
            x = (x > 0.9505f) ? 0.9505f : ((x < 0f) ? 0f : x);
            y = (y > 1.0f) ? 1.0f : ((y < 0f) ? 0f : y);
            z = (z > 1.089f) ? 1.089f : ((z < 0f) ? 0f : z);
            LABColor lab = new LABColor(0f, 0f, 0f);
            float fx = x / D65x;
            float fy = y / D65y;
            float fz = z / D65z;
            fx = ((fx > 0.008856f) ? (float)Math.Pow(fx, (1.0f / 3.0f)) : (7.787f * fx + 16.0f / 116.0f));
            fy = ((fy > 0.008856f) ? (float)Math.Pow(fy, (1.0f / 3.0f)) : (7.787f * fy + 16.0f / 116.0f));
            fz = ((fz > 0.008856f) ? (float)Math.Pow(fz, (1.0f / 3.0f)) : (7.787f * fz + 16.0f / 116.0f));
            lab.L = 116.0f * fy - 16f;
            lab.A = 500.0f * (fx - fy);
            lab.B = 200.0f * (fy - fz);
            return lab;
        }

        public static Color ToColor(LABColor lab)
        {
            float D65x = 0.9505f;
            float D65y = 1.0f;
            float D65z = 1.0890f;
            float delta = 6.0f / 29.0f;
            float fy = (lab.L + 16f) / 116.0f;
            float fx = fy + (lab.A / 500.0f);
            float fz = fy - (lab.B / 200.0f);
            float x = (fx > delta) ? D65x * (fx * fx * fx) : (fx - 16.0f / 116.0f) * 3f * (delta * delta) * D65x;
            float y = (fy > delta) ? D65y * (fy * fy * fy) : (fy - 16.0f / 116.0f) * 3f * (delta * delta) * D65y;
            float z = (fz > delta) ? D65z * (fz * fz * fz) : (fz - 16.0f / 116.0f) * 3f * (delta * delta) * D65z;
            float r = x * 3.2410f - y * 1.5374f - z * 0.4986f;
            float g = -x * 0.9692f + y * 1.8760f - z * 0.0416f;
            float b = x * 0.0556f - y * 0.2040f + z * 1.0570f;
            r = (r <= 0.0031308f) ? 12.92f * r : (1f + 0.055f) * (float)Math.Pow(r, (1.0f / 2.4f)) - 0.055f;
            g = (g <= 0.0031308f) ? 12.92f * g : (1f + 0.055f) * (float)Math.Pow(g, (1.0f / 2.4f)) - 0.055f;
            b = (b <= 0.0031308f) ? 12.92f * b : (1f + 0.055f) * (float)Math.Pow(b, (1.0f / 2.4f)) - 0.055f;
            r = (r < 0) ? 0 : r;
            g = (g < 0) ? 0 : g;
            b = (b < 0) ? 0 : b;
            return Color.FromArgb((int)r, (int)g, (int)b);
        }

        public Color ToColor()
        {
            return LABColor.ToColor(this);
        }

        public override string ToString()
        {
            return "L:" + L + " A:" + A + " B:" + B;
        }
    }
}
