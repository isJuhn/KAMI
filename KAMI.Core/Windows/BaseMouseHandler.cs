using KAMI.Core.Common;
using System;
using System.Runtime.InteropServices;

namespace KAMI.Core.Windows
{
    internal abstract class BaseMouseHandler : IMouseHandler
    {
        [DllImport("user32.dll")]
        protected static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        protected static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        protected static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ClipCursor(ref RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ClipCursor(IntPtr lpRect);

        public virtual (int, int) GetCenterDiff()
        {
            throw new NotImplementedException();
        }

        public void ConfineCursor()
        {
            IntPtr handle = GetForegroundWindow();
            if (GetClientRect(handle, out RECT rcClip))
            {
                POINT origin = new POINT(0, 0);
                if (ClientToScreen(handle, ref origin))
                {
                    rcClip.Location = origin;
                    ClipCursor(ref rcClip);
                }
            }
        }

        public void ReleaseCursor()
        {
            ClipCursor(IntPtr.Zero);
        }
    }
}
