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
    class Win32Struct
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct MouseInput
        {
            public Int32 dx;
            public Int32 dy;
            public Int32 Mousedata;
            public Int32 dwFlag;
            public Int32 time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct tagKEYBDINPUT
        {
            Int16 wVk;
            Int16 wScan;
            Int32 dwFlags;
            Int32 time;
            IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct tagHARDWAREINPUT
        {
            Int32 uMsg;
            Int16 wParamL;
            Int16 wParamH;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct Input
        {
            [FieldOffset(0)]
            public Int32 type;
            [FieldOffset(4)]
            public MouseInput mi;
            [FieldOffset(4)]
            public tagKEYBDINPUT ki;
            [FieldOffset(4)]
            public tagHARDWAREINPUT hi;
        }
    }
}
