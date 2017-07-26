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
    public partial class mainTouchpad : WMTouchForm
    {
        enum touchSignal
        {
            movMouse = 0,
            movTouchBoard = 1,
            click = 2,
            doubleClick = 3,
        }
        const int WM_MOUSEHOVER = 0x02A1;


        const int MOUSEEVENTF_MOVE = 0x0001;      //移动鼠标 
        const int MOUSEEVENTF_LEFTDOWN = 0x0002; //模拟鼠标左键按下 
        const int MOUSEEVENTF_LEFTUP = 0x0004; //模拟鼠标左键抬起 
        const int MOUSEEVENTF_RIGHTDOWN = 0x0008; //模拟鼠标右键按下 
        const int MOUSEEVENTF_RIGHTUP = 0x0010; //模拟鼠标右键抬起 
        const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;//模拟鼠标中键按下
        const int MOUSEEVENTF_MIDDLEUP = 0x0040; //模拟鼠标中键抬起 
        const int MOUSEEVENTF_ABSOLUTE = 0x8000; //标示是否采用绝对坐标 
        const uint infinit = 0xffffffff;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int mouse_event(int dwFlags,
            int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("User32")]
        public extern static void SetCursorPos(int x, int y);

        [DllImport("User32")]
        public extern static IntPtr SendMessage(
            IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr CreateEvent(IntPtr lpEventAttributes,
        bool bManualReset, bool bInitialState, IntPtr lpName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern uint WaitForSingleObject(
            IntPtr hHandle, uint dwMilliseconds);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool SetEvent(IntPtr hEvent);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool ResetEvent(IntPtr hEvent);


        [DllImport("user32.dll", EntryPoint = "WindowFromPoint")]
        public static extern IntPtr WindowFromPoint(Point p);

        [DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow(); //获得本窗体的句柄

        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);//设置此窗体为活动窗体

        const int PROPORTION_ALL = 5;//占据屏幕1/5
        const int PROPORTION_BUTTON = 4;//占据pointNowLoc屏幕1/4，黄金比例0.618
        const double goldenRatio = 0.618;

        Rectangle ScreenArea;
        Rectangle recTouchBoard;
        Rectangle recBtnLeft, recBtnRight;


        Rectangle recMainSeperator, recBtnSeperator;

        IntPtr hEvent;
        //这一组是虚拟鼠标上一时刻和此时刻的位置
        public Point pointNowLoc = new Point(0, 0);
        Point pointLastLoc = new Point(0, 0);

        //这一组是虚拟触摸板手指的上一时刻和此时刻位置
        Point pointTouchNowLoc;
        Point pointTouchLastLoc = new Point(0, 0);
        Thread oGetArgThread;
        //设置计时器来判断状态
        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

        bool isMove = false;

        public mainTouchpad()
        {
            InitializeComponent();
        }

        private int calculate(int a, int b)
        {
            int c = a - b, e;
            e = (c > 0) ? 1 : -1;
            return (int)(e * Math.Pow(Math.Abs(c), 1.7));
        }

        private void updatePicLoc()
        {
            while (true)
            {
                SetForegroundWindow(this.Handle);

            }
        }

        private void mainTouchpad_paint(object sender, PaintEventArgs e)
        {
            Color myColor;
            myColor = Color.FromArgb(192, 192, 192);
            Brush bsh = new SolidBrush(myColor);
            Graphics g = e.Graphics;
            g.FillRectangle(bsh, recBtnLeft);

            //填充左，右按钮
            myColor = Color.FromArgb(192, 192, 192);
            bsh = new SolidBrush(myColor);
            g.FillRectangle(bsh, recBtnRight);

            myColor = Color.FromArgb(192, 192, 192);
            bsh = new SolidBrush(myColor);
            g.FillRectangle(bsh, recTouchBoard);

            //填充分割线段
            myColor = Color.FromArgb(128, 128, 128);
            bsh = new SolidBrush(myColor);
            g.FillRectangle(bsh, recMainSeperator);

            myColor = Color.FromArgb(128, 128, 128);
            bsh = new SolidBrush(myColor);
            g.FillRectangle(bsh, recBtnSeperator);

        }

        private bool isInRectangle(Point p, Rectangle rec)
        {

            if (p.X > rec.X &&
                p.Y > rec.Y &&
                p.X < rec.X + rec.Width &&
                p.Y < rec.Y + rec.Height)
            {

                return true;
            }
            else return false;
        }



        [DllImport("user32.dll", EntryPoint = "ShowCursor", CharSet = CharSet.Auto)]
        public static extern void ShowCursor(int status);

        private void OnTouchDownHandler(object sender, WMTouchEventArgs e)
        {

            if (isInRectangle(new Point(e.LocationX, e.LocationY), recBtnLeft))
            {
                //label1.Text = (String.Format("down: {0} {1}", e.LocationX,e.LocationY));
                IntPtr hwnd = WindowFromPoint(new Point(pointNowLoc.X, pointNowLoc.Y));

                SetForegroundWindow(hwnd);

                mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP | MOUSEEVENTF_ABSOLUTE,
                    pointNowLoc.X, pointNowLoc.Y, 0, (int)hwnd);

            }

            if (isInRectangle(new Point(e.LocationX, e.LocationY), recBtnRight))
            {
                this.Close();
                oGetArgThread.Abort();
            }

            if (isInRectangle(new Point(e.LocationX, e.LocationY), recTouchBoard))
            {
                //***pointTouchLastLoc = new Point(0, 0);
                //***pointNowLoc = pointLastLoc;
                pointTouchLastLoc = new Point(0, 0);
                pointNowLoc = pointLastLoc;
            }

        }
        private void OnTouchUpHandler(object sender, WMTouchEventArgs e)
        {
            if (isInRectangle(new Point(e.LocationX, e.LocationY), recTouchBoard))
            {
                pointLastLoc = pointNowLoc;
                pointTouchLastLoc = new Point(0, 0);
            }
            else if (isInRectangle(new Point(e.LocationX, e.LocationY), recBtnLeft))
            {
                IntPtr hwnd = WindowFromPoint(new Point(pointNowLoc.X, pointNowLoc.Y));

                SetForegroundWindow(hwnd);
                mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP,
                        0, 0, 0, 0);
            }


        }

        const ushort WM_MOUSEMOVE = 0x0200;
        const ushort WM_MBUTTONDOWN = 0x0207;
        const ushort WM_MOUSEACTIVATE = 0x0021;
        const ushort WM_LBUTTONDOWN = 0x0201;
        const ushort WM_LBUTTONUP = 0x0202;
        const ushort WM_CAPTURECHANGED = 0x0215;
        const ushort WM_LBUTTONDBLCLK = 0x0203;
        const ushort WM_XBUTTONDOWN = 0x020B;
        const ushort WM_NCXBUTTONDOWN = 0x00AB;
        const ushort WM_NCRBUTTONDOWN = 0x00A4;
        const ushort WM_NCMBUTTONDOWN = 0x00A7;
        const ushort WM_NCLBUTTONDOWN = 0x00A1;
        protected override void DefWndProc(ref System.Windows.Forms.Message m)
        {
            //每一次消息循环都设置鼠标位置
            SetCursorPos(pointNowLoc.X, pointNowLoc.Y);
            switch (m.Msg)
            {

                default:
                    base.DefWndProc(ref m);//一定要调用基类函数，以便系统处理其它消息。
                    break;
            }

        }


        private void OnTouchMoveHandler(object sender, WMTouchEventArgs e)
        {

            if (isInRectangle(new Point(e.LocationX, e.LocationY), recTouchBoard))
            {
                //判断是否是第一次触摸
                if (pointTouchLastLoc == new Point(0, 0))
                {
                    pointTouchLastLoc = new Point(e.LocationX, e.LocationY);
                    pointTouchNowLoc = pointTouchLastLoc;
                }
                pointTouchNowLoc = new Point(e.LocationX, e.LocationY);
                pointNowLoc.X += calculate(pointTouchNowLoc.X, pointTouchLastLoc.X);
                pointNowLoc.Y += calculate(pointTouchNowLoc.Y, pointTouchLastLoc.Y);
                //发布移动鼠标事件

                //根据需要判断是要设置为当前焦点 SetForegroundWindow(hwnd);
                //SetForegroundWindow(this.Handle);
                //设置鼠标移动事件
                //SetForegroundWindow(this.Handle);
                //设置鼠标移动事件
                mouse_event(MOUSEEVENTF_MOVE,
                    pointNowLoc.X, pointNowLoc.X, 0, 0);
                // SetCursorPos(pointTouchNowLoc.X, pointTouchNowLoc.Y);
                pointTouchLastLoc = pointTouchNowLoc;
            }

        }

        private void hotKey(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Escape)
            {
                this.Close();
                oGetArgThread.Abort();
            }
        }

        private void mainTouchpad_MouseEnter(object sender, EventArgs e)
        {
            SetCursorPos(pointNowLoc.X, pointNowLoc.Y);
        }

        private void mainTouchpad_load(object sender, EventArgs e)
        {

            Touchdown += OnTouchDownHandler;
            Touchup += OnTouchUpHandler;
            TouchMove += OnTouchMoveHandler;

            Control.CheckForIllegalCrossThreadCalls = false;

            oGetArgThread = new Thread(new ThreadStart(updatePicLoc));
            oGetArgThread.Start();
            hEvent = CreateEvent((IntPtr)null, true, false, (IntPtr)null);

            this.StartPosition = FormStartPosition.Manual;
            this.TopMost = true;
            this.Opacity = 0.9;
            this.Hide();

            ScreenArea = System.Windows.Forms.Screen.GetBounds(this);
            recTouchBoard = new Rectangle(
                new Point(
                    0,
                    0
                ),
                new Size(
                    ScreenArea.Width / PROPORTION_ALL,
                    (int)(ScreenArea.Width * goldenRatio / PROPORTION_ALL)
                )
            );
            //设置鼠标从触摸板左上角
            pointTouchLastLoc = recTouchBoard.Location;

            //设置无边框
            this.FormBorderStyle = FormBorderStyle.None;
            //this.WindowState = FormWindowState.Maximized;
            //this.BackColor = Color.White;
            //this.TransparencyKey = Color.White;
            recBtnLeft = new Rectangle(
                new Point(
                    recTouchBoard.X,
                    recTouchBoard.Y + recTouchBoard.Height * 3 / 4
                 ),
                new Size(
                   recTouchBoard.Width / 2,
                   recTouchBoard.Height / 4
                )
            );

            recBtnRight = new Rectangle(
                new Point(
                    recTouchBoard.X + recTouchBoard.Width / 2,
                    recTouchBoard.Y + recTouchBoard.Height * 3 / 4
                ),
                new Size(
                    recTouchBoard.Width / 2,
                    recTouchBoard.Height / 4
                )
            );

            recMainSeperator = new Rectangle(
                new Point(
                    recTouchBoard.X,
                    recTouchBoard.Y + recTouchBoard.Height * 3 / 4 - recMainSeperator.Height / 2
                ),
                new Size(
                    recTouchBoard.Width,
                    recTouchBoard.Height / 75
                )
            );

            recBtnSeperator = new Rectangle(
                new Point(
                    recTouchBoard.X + recTouchBoard.Width / 2 - recBtnSeperator.Width / 2,
                    recTouchBoard.Y + recTouchBoard.Height * 3 / 4
                ),
                new Size(
                    recMainSeperator.Height,
                    recBtnRight.Height
                )
            );

            //窗体的位置由Location属性决定
            this.Location = (Point)new Size(ScreenArea.Width - recTouchBoard.Width, ScreenArea.Height - recTouchBoard.Height - 40);
            this.Size = new Size(recTouchBoard.Width, recTouchBoard.Height);
            recTouchBoard.Height = (recTouchBoard.Height * 3) / 4;
            this.Show();
        }
    }
}
