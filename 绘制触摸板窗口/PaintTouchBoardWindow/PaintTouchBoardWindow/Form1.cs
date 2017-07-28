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

namespace PaintTouchBoardWindow
{
    public partial class Form1 : WMTouchForm
    {
        const int PROPORTION_ALL = 5;//占据屏幕1/5
        const int PROPORTION_BUTTON = 4;//占据pointNowLoc屏幕1/4，黄金比例0.618
        const double goldenRatio = 0.618;

        static Rectangle ScreenArea;
        static Rectangle recTouchBoard;
        static Rectangle recBtnLeft, recBtnRight;
        static Rectangle recMainSeperator, recBtnSeperator;

        static bool isSendMouseState = false;

        //要发送的按键状态
        MouseState state;
        //发送x,y或者dx,dy;
        int x; int y;
        //用来发送消息的线程
        Thread threadendData;

        //描述发送鼠标状态的消息
        enum MouseState
        {
            move=0,
            leftDown=1,
            rightDown=2,
            touchUp = 3,
        }

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
            //添加手指按下，抬起，移动的方法
            Touchdown += OnTouchDownHandler;
            Touchup += OnTouchUpHandler;
            TouchMove += OnTouchMoveHandler;

            //开启新线程来发送触摸消息
            threadendData = new Thread(SendMouseState);
            threadendData.Start();
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
            this.Opacity = 0.9;//设置透明度
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
            if(isInRectangle(new Point(e.LocationX, e.LocationY), recBtnLeft))
            {
                isSendMouseState = true;
                state = MouseState.leftDown;
            }

            else if(isInRectangle(new Point(e.LocationX, e.LocationY), recBtnRight))
            {
                isSendMouseState = true;
                state = MouseState.rightDown;
            }
        }

        private void OnTouchUpHandler(object sender, WMTouchEventArgs e)
        {
            if (isInRectangle(new Point(e.LocationX, e.LocationY), recTouchBoard)||
                isInRectangle(new Point(e.LocationX, e.LocationY),recBtnLeft)||
                isInRectangle(new Point(e.LocationX, e.LocationY),recBtnRight))
            {
                isSendMouseState = true;
                state = MouseState.touchUp;
            }
        }

        private void OnTouchMoveHandler(object sender, WMTouchEventArgs e)
        {
            if (isInRectangle(new Point(e.LocationX, e.LocationY), recTouchBoard))
            {
                isSendMouseState = true;
                state = MouseState.move;
                x = e.LocationX;
                y = e.LocationY;
            }
        }

        private void SendMouseState()
        {
            using (NamedPipeServerStream pipeStream = new NamedPipeServerStream("testpipe"))
            {
                pipeStream.WaitForConnection();

                using (StreamWriter writer = new StreamWriter(pipeStream))
                {
                    writer.AutoFlush = true;

                    while (true)
                    {
                        if(isSendMouseState == true)
                        {
                            writer.WriteLine("#" + state + "#" + x + "#" + y);
                            isSendMouseState = false;
                        }
                        
                    }
                }
            }
        }

        private void Form1_paint(object sender, PaintEventArgs e)
        {
            Painter.PaintRectangle(e, 192, 192, 192, recBtnLeft);
            Painter.PaintRectangle(e, 192, 192, 192, recBtnRight);
            Painter.PaintRectangle(e, 192, 192, 192, recTouchBoard);
            Painter.PaintRectangle(e, 128, 128, 128, recMainSeperator);
            Painter.PaintRectangle(e, 128, 128, 128, recBtnSeperator);
        }
    }
}
