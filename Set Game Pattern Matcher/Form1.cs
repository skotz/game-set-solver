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
        List<SetCardMatch> matches;

        public Form1()
        {
            InitializeComponent();

            set = new SetGame();
            set.OnDebugImage += set_OnDebugImage;
            set.OnFindMatches += set_OnFindMatches;
        }

        void set_OnDebugImage(object sender, Bitmap e)
        {
            pictureBox1.Image = e;
        }

        void set_OnFindMatches(object sender, List<SetCardMatch> e)
        {
            matches = e;
            imageRotateTimer.Start();
        }

        private void imageRotateTimer_Tick(object sender, EventArgs e)
        {
            if (matches.Count > 0)
            {
                pictureBox1.Image = matches[0].Image;

                matches.Add(matches[0]);
                matches.RemoveAt(0);
            }
        }

        private void loadImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openSetImage.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Bitmap setImage = (Bitmap)Bitmap.FromFile(openSetImage.FileName);
                    set.GetMatchesAsync(setImage, (int)numericUpDown1.Value);
                }
                catch (Exception)
                {
                    MessageBox.Show("Could not load file! " + openSetImage.FileName, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void test1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap b = (Bitmap)Bitmap.FromFile("../../../Test Images/Google Images/setcards.jpg");

            set.GetMatchesAsync(b, (int)numericUpDown1.Value);
        }

        private void test2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap b = (Bitmap)Bitmap.FromFile("../../../Test Images/Google Images/set2.jpg");

            set.GetMatchesAsync(b, (int)numericUpDown1.Value);
        }

        private void test3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap b = (Bitmap)Bitmap.FromFile("../../../Test Images/Pictures/test-03.png");

            set.GetMatchesAsync(b, (int)numericUpDown1.Value);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Scott Clayton 2016\r\nhttps://github.com/skotz/game-set-solver", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
