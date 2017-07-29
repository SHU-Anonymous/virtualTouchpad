using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Pipes;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

namespace PaintTouchBoardWindow
{
    public partial class Form1 : WMTouchForm
    {
        const int PROPORTION_ALL = 3;//占据屏幕1/5
        const int PROPORTION_BUTTON = 4;//占据pointNowLoc屏幕1/4，黄金比例0.618
        const double goldenRatio = 0.618;

        static Rectangle ScreenArea;
        static Rectangle recTouchBoard;
        static Rectangle recBtnLeft, recBtnRight;
        static Rectangle recMainSeperator, recBtnSeperator;

        //当前鼠标所在位置
        static Point nowMouseLoc = new Point(0, 0);
        //上个时刻鼠标所在位置
        static Point lastMouseLoc = new Point(0, 0);
        //此时手指所在位置
        static Point nowFingerLoc = new Point(0, 0);
        //上一时刻手指所在位置
        static Point lastFingerLoc = new Point(0, 0);

        public Form1()
        {
            InitializeComponent();
        }
        //设置窗口为失去焦点的状态
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= Win32Constant.WS_EX_NOACTIVATE;
                return cp;
            }
        }

        protected override void DefWndProc(ref System.Windows.Forms.Message m)
        {
            Win32Api.SetCursorPos(nowMouseLoc.X, nowMouseLoc.Y);
            switch (m.Msg)
            {
                default:
                    base.DefWndProc(ref m);//一定要调用基类函数，以便系统处理其它消息。
                    break;
            }
        }


        //设置窗口为置顶状态
        private void Form1_load(object sender, EventArgs e)
        {   
            //避免非gui线程报错
            Control.CheckForIllegalCrossThreadCalls = false;
            //计算相关矩形大小
            simulateRectangles();
            //设置窗口的一些信息
            setFormCondition();
            //touchboard高度分了1/4给button
            recTouchBoard.Height = (recTouchBoard.Height * 3) / 4;
            //更新窗口，设置为置顶并且不获取焦点
            updateWindow();
            //添加手指按下，抬起，移动的方法
            Touchdown += OnTouchDownHandler;
            Touchup += OnTouchUpHandler;
            TouchMove += OnTouchMoveHandler;

        }

        //计算touchboard 和buttons所占据的矩形
        private void simulateRectangles()
        {   //获取屏幕区域大小
            ScreenArea = System.Windows.Forms.Screen.GetBounds(this);
            //触摸板矩形的位置位置和大小
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

            //左按钮矩形的位置位置和大小
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

            //右按钮矩形的位置位置和大小
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

            //主分割线的位置和大小
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

            //按钮分割线的位置和大小
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
        }

        //设置Form当前状态
        private void setFormCondition()
        {
            this.Hide();//窗口初始化为隐藏状态
            this.TopMost = true;//置为最顶层
            this.FormBorderStyle = FormBorderStyle.None;//设置为无边框
            //窗口位置
            this.Location = (Point)new Size(ScreenArea.Width - recTouchBoard.Width, ScreenArea.Height - recTouchBoard.Height - 40);
            //窗口大小
            this.Size = new Size(recTouchBoard.Width, recTouchBoard.Height - 1);
            //显示窗口
            this.Show();
            //重置touchboard高度，因为分了1/4给按钮
        }

        //判断某个点是否在某个区域内
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

        private void OnTouchDownHandler(object sender, WMTouchEventArgs e)
        {
            //按下触摸板只需考虑两种情况：在左按钮 还是在 右按钮
            if(isInRectangle(new Point(e.LocationX, e.LocationY), recTouchBoard))
            {
                myMouseManager(Win32Constant.MOUSEEVENTF_LEFTDOWN);
            }


            if(isInRectangle(new Point(e.LocationX, e.LocationY), recBtnLeft))
            {
                for (int i = 0; i < 10; i++)
                {
                    Win32Api.SetCursorPos(nowMouseLoc.X, nowMouseLoc.Y);
                    Thread.Sleep(10);
                }
                myMouseManager(Win32Constant.MOUSEEVENTF_LEFTDOWN);
                for (int i = 0; i < 10; i++)
                {
                    Win32Api.SetCursorPos(nowMouseLoc.X, nowMouseLoc.Y);
                    Thread.Sleep(5);
                }
                myMouseManager(Win32Constant.MOUSEEVENTF_LEFTUP);
            }
            //右按钮
            else if(isInRectangle(new Point(e.LocationX, e.LocationY), recBtnRight))
            {
                Win32Api.SetCursorPos(nowMouseLoc.X, nowMouseLoc.Y);
                myMouseManager(Win32Constant.MouseEvent_RightDown | Win32Constant.MouseEvent_RightUp);
                Win32Api.SetCursorPos(nowMouseLoc.X, nowMouseLoc.Y);

                Win32Api.SetForegroundWindow(this.Handle);
                Win32Api.SetCursorPos(nowMouseLoc.X + 1, nowMouseLoc.Y + 1);
            }
        }

        private void OnTouchUpHandler(object sender, WMTouchEventArgs e)
        {
            if (isInRectangle(new Point(e.LocationX, e.LocationY), recTouchBoard)||
                isInRectangle(new Point(e.LocationX, e.LocationY),recBtnLeft)||
                isInRectangle(new Point(e.LocationX, e.LocationY),recBtnRight))
            {
                lastFingerLoc = new Point(0, 0);
            }
            myMouseManager(Win32Constant.MOUSEEVENTF_LEFTUP);
        }

        private void OnTouchMoveHandler(object sender, WMTouchEventArgs e)
        {
            if (isInRectangle(new Point(e.LocationX, e.LocationY), recTouchBoard))
            {
                nowFingerLoc = new Point(e.LocationX, e.LocationY);

                //lastFingerLoc为0有两种情况，一是刚开始，二是之前手指抬起来
                if (lastFingerLoc == new Point(0, 0))
                {
                    lastFingerLoc = new Point(nowFingerLoc.X, nowFingerLoc.Y);
                }

                //通过之前手指位置和当前手指位置算出当前鼠标位置
                nowMouseLoc = new Point(nowMouseLoc.X + calculate(nowFingerLoc.X, lastFingerLoc.X),
                        nowMouseLoc.Y + calculate(nowFingerLoc.Y, lastFingerLoc.Y));
                lastFingerLoc = new Point(nowFingerLoc.X, nowFingerLoc.Y);
                //设置鼠标位置
                myMouseManager(Win32Constant.MouseEvent_Move);
                Win32Api.SetCursorPos(nowMouseLoc.X, nowMouseLoc.Y);
            }
        }


        public static void myMouseManager(int flag)
        {
            Win32Struct.MouseInput myMinput = new Win32Struct.MouseInput();
            //获取屏幕高度和宽度
            int ScreenWidth = Screen.PrimaryScreen.WorkingArea.Width;
            int ScreenHeight = Screen.PrimaryScreen.WorkingArea.Height;
            //转化为像素坐标
            myMinput.dx = nowMouseLoc.X * (65335 / ScreenWidth);
            myMinput.dy = nowMouseLoc.Y * (65335 / ScreenHeight);

            myMinput.Mousedata = 1000000000;
            //设置鼠标工作模式
            myMinput.dwFlag = flag;
            myMinput.time = 0;
            Win32Struct.Input[] myInput = new Win32Struct.Input[1];
            myInput[0] = new Win32Struct.Input();
            myInput[0].type = 0;
            myInput[0].mi = myMinput;
            //发送鼠标消息
            UInt32 result = Win32Api.SendInput((uint)myInput.Length,
                myInput, Marshal.SizeOf(myInput[0].GetType()));
            //错误输出
            if (result == 0)
            {
                MessageBox.Show("myMouseManager fail");
            }
        }

        static private int calculate(int a, int b)
        {
            int c = a - b, e;
            e = (c > 0) ? 1 : -1;
            return (int)(e * Math.Pow(Math.Abs(c), 1.5));
        }

        private void updateWindow()
        {
            
            Win32Api.SetWindowPos(this.Handle, -1,
                0,0,
                this.Width, this.Height, Win32Constant.SWP_NOACTIVATE |
                Win32Constant.SWP_NOSIZE | Win32Constant.WP_NOMOVE );
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            updateWindow();
        }

        private void Form_Click(object sender, EventArgs e)
        {
            updateWindow();
        }

        private void Form1_paint(object sender, PaintEventArgs e)
        {
            Painter.PaintRectangle(e, 192, 192, 192, recBtnLeft);
            Painter.PaintRectangle(e, 192, 192, 192, recBtnRight);
            Painter.PaintRectangle(e, 192, 192, 192, recTouchBoard);
            Painter.PaintRectangle(e, 64, 64, 64, recMainSeperator);
            Painter.PaintRectangle(e, 64, 64, 64, recBtnSeperator);
        }
    }
}
