using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

namespace PaintTouchBoardWindow
{
    class Painter
    {
        public static void PaintRectangle(PaintEventArgs e,int a,int b,int c,Rectangle rec)
        {
            Color myColor;
            myColor = Color.FromArgb(a, b, c);
            Brush bsh = new SolidBrush(myColor);
            Graphics g = e.Graphics;
            g.FillRectangle(bsh, rec);
        }
    }
}
