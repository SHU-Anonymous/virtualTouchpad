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

namespace ReceiveData
{
    class Program
    {
        static Point nowMouseLoc = new Point(0, 0);
        static Point lastMouseLoc = new Point(0, 0);
        static Point nowFingerLoc = new Point(0, 0);
        static Point lastFingerLoc = new Point(0, 0);

        static Thread threadUpdateCur;
        static bool s = false;
        enum MouseState
        {
            move = 0,
            leftDown = 1,
            rightDown = 2,
            touchUp = 3,
        }

        static MouseState nowMouseState = MouseState.move;

        static private int calculate(int a, int b)
        {
            int c = a - b, e;
            e = (c > 0) ? 1 : -1;
            return (int)(e * Math.Pow(Math.Abs(c), 1.7));
        }

        public static void func()
        {
            while (true)
            {
               // IntPtr hwnd = Win32Api.WindowFromPoint(nowMouseLoc);
                //Win32Api.SetForegroundWindow(hwnd);
                if (s == false)
                {
                    Win32Api.SetCursorPos(nowMouseLoc.X, nowMouseLoc.Y);
                    Win32Api.mouse_event(Win32Constant.MOUSEEVENTF_MOVE, 1, 0, 0, (IntPtr)0);
                }
                    
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

            myMinput.Mousedata = 0;
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
                MessageBox.Show("fail");
            }
        }

        static void Main(string[] args)
        {
            threadUpdateCur = new Thread(func);
            threadUpdateCur.Start();
            using (NamedPipeClientStream pipeStream = new NamedPipeClientStream("testpipe"))
            {
                pipeStream.Connect();
                //在client读取server端写的数据
                using (StreamReader rdr = new StreamReader(pipeStream))
                {
                    //获取并且记录下当前鼠标位置

                    string temp;
                    while ((temp = rdr.ReadLine()) != "")
                    {
                       // Win32Struct.POINT p = new Win32Struct.POINT(0, 0);
                       // Win32Api.GetCursorPos(ref p);
                       // nowMouseLoc = new Point(p.X, p.Y);
                        Console.WriteLine("mouse locatiol: {0},{1}", nowMouseLoc.X, nowMouseLoc.X);

                        Console.WriteLine("{0}:{1}", DateTime.Now, temp);
                        if (temp[1] == 'm')
                        {
                            s = true;
                            nowMouseState = MouseState.move;
                            //'#' + "move" + '#'
                            
                            int i = 1 + "move".Length+1;
                            string strX = null;
                            while (temp[i] != '#')
                            {
                                strX += temp[i];
                                i++;
                            }
                            
                            string strY = null;
                            for (int t = i+1; t < temp.Length; t++)
                            {
                                strY += temp[t];
                            }
                            //记录当前触摸板位置
                            nowFingerLoc = new Point(int.Parse(strX), int.Parse(strY));

                            //lastFingerLoc为0有两种情况，一是刚开始，二是之前手指抬起来
                            if (lastFingerLoc==new Point(0, 0))
                            {
                                lastFingerLoc = new Point(nowFingerLoc.X, nowFingerLoc.Y);
                            }

                            //通过之前手指位置和当前手指位置算出当前鼠标位置
                            nowMouseLoc = new Point(nowMouseLoc.X + calculate(nowFingerLoc.X, lastFingerLoc.X),
                                    nowMouseLoc.Y + calculate(nowFingerLoc.Y, lastFingerLoc.Y));
                            lastFingerLoc = new Point(nowFingerLoc.X, nowFingerLoc.Y);
                            //设置鼠标位置
                            myMouseManager(Win32Constant.MouseEvent_Absolute | Win32Constant.MouseEvent_Move);
                            //test
                            s = false;

                        }

                        else if (temp[1] == 'l')
                        {
                            nowMouseState = MouseState.leftDown;
                            //myMouseManager(Win32Constant.MouseEvent_Absolute | Win32Constant.MouseEvent_Move);
                            //myMouseManager(Win32Constant.MouseEvent_LeftDown);

                            //IntPtr hwnd = Win32Api.WindowFromPoint(nowMouseLoc);
                            //Win32Api.SetForegroundWindow(hwnd);

                            Win32Api.SetCursorPos(nowMouseLoc.X,nowMouseLoc.Y);
                            myMouseManager(Win32Constant.MouseEvent_Absolute
                                |Win32Constant.MOUSEEVENTF_LEFTDOWN | Win32Constant.MOUSEEVENTF_LEFTUP);
                            myMouseManager(Win32Constant.MouseEvent_Absolute
                                | Win32Constant.MOUSEEVENTF_LEFTDOWN | Win32Constant.MOUSEEVENTF_LEFTUP);
                            Win32Api.SetCursorPos(nowMouseLoc.X, nowMouseLoc.Y);

                            for (int q = 0; q < 5; q++)
                            {
                                Win32Api.SetCursorPos(nowMouseLoc.X, nowMouseLoc.Y);
                                Win32Api.mouse_event(Win32Constant.MOUSEEVENTF_MOVE, 0, 1, 0, (IntPtr)0);
                            }
                        }

                        else if (temp[1] == 'r')
                        {
                            nowMouseState = MouseState.rightDown;
                            Win32Api.SetCursorPos(nowMouseLoc.X, nowMouseLoc.Y);
                            myMouseManager(Win32Constant.MouseEvent_RightDown| Win32Constant.MouseEvent_RightUp);
                            Win32Api.SetCursorPos(nowMouseLoc.X, nowMouseLoc.Y);
                            // Win32Api.mouse_event(Win32Constant.MOUSEEVENTF_RIGHTDOWN | Win32Constant.MOUSEEVENTF_RIGHTUP,
                            //     0, 0, 0, (IntPtr)0);
                            IntPtr hwnd = Win32Api.WindowFromPoint(nowMouseLoc);
                            Win32Api.SetForegroundWindow(hwnd);

                            for (int q = 0; q < 100; q++)
                            {
                                nowMouseLoc.X += 1;
                                nowMouseLoc.Y += 1;
                                Win32Api.SetCursorPos(nowMouseLoc.X, nowMouseLoc.Y);
                                Win32Api.mouse_event(Win32Constant.MOUSEEVENTF_MOVE, 0, 1, 0, (IntPtr)0);
                                Thread.Sleep(100);
                            }

                        }
                        //手指抬起
                        else if (temp[1] == 't')
                        {
                            lastFingerLoc = new Point(0, 0);
                        }

                        else
                        {
                            MessageBox.Show("无法识别enum");
                        }

                    }
                }
            }

            Console.Read();

        }
    }
}
