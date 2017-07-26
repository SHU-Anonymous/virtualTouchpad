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

namespace virtualTouchpad
{
    public partial class mainTouchpad : Form
    {
        enum touchSignal
        {
            movMouse = 0,
            movTouchpad = 1,
            Click = 2,
            doubleClick = 3,
        }
        const int WM_MOUSEHOVER = 0x02A1;


        //const int WM_MOUSEHOVER = 0x02A1;
        const uint infinit = 0xffffffff;



        public mainTouchpad()
        {
            InitializeComponent();
        }
    }
}
