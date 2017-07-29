using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintTouchBoardWindow
{
    class Win32Constant
    {
        public const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        public const int MOUSEEVENTF_LEFTUP = 0x0004;
        public const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        public const int MOUSEEVENTF_RIGHTUP = 0x0010;
        public const int MOUSEEVENTF_MOVE = 0x0001;
        public const int MOUSEEVENTF_ABSOLUTE = 0x8000;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int MK_LBUTTON = 0x0001;

        public const int MouseEvent_Absolute = 0x8000;
        public const int MouserEvent_Hwheel = 0x01000;
        public const int MouseEvent_Move = 0x0001;
        public const int MouseEvent_Move_noCoalesce = 0x2000;
        public const int MouseEvent_LeftDown = 0x0002;
        public const int MouseEvent_LeftUp = 0x0004;
        public const int MouseEvent_MiddleDown = 0x0020;
        public const int MouseEvent_MiddleUp = 0x0040;
        public const int MouseEvent_RightDown = 0x0008;
        public const int MouseEvent_RightUp = 0x0010;
        public const int MouseEvent_Wheel = 0x0800;
        public const int MousseEvent_XUp = 0x0100;
        public const int MousseEvent_XDown = 0x0080;

        //用来设置窗口不获取焦点
        public const int WS_EX_NOACTIVATE = 0x08000000;

        public const int SWP_NOACTIVATE = 0x0010;
        public const int SWP_NOSIZE = 1;
        public const int WP_NOMOVE = 2;
    }
}
