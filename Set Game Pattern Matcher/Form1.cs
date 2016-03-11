using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Set_Game_Pattern_Matcher
{
    public partial class Form1 : Form
    {
        SetGame set;

        public Form1()
        {
            InitializeComponent();

            set = new SetGame();
            set.OnDebugImage += set_OnDebugImage;
        }

        private void set_OnDebugImage(object sender, Bitmap e)
        {
            pictureBox1.Image = e;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Bitmap b = (Bitmap)Bitmap.FromFile("../../../Test Images/Google Images/setcards.jpg");

            set.GetMatches(b, (int)numericUpDown1.Value);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Bitmap b = (Bitmap)Bitmap.FromFile("../../../Test Images/Google Images/set2.jpg");

            set.GetMatches(b, (int)numericUpDown1.Value);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Bitmap b = (Bitmap)Bitmap.FromFile("../../../Test Images/Pictures/test-03.png");

            set.GetMatches(b, (int)numericUpDown1.Value);
        }
    }
}
