using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace KAMI.Core
{
    public static class MouseCursor
    {
        [DllImport("user32.dll")]
        static extern IntPtr CreateCursor(IntPtr hInst, int xHotSpot, int yHotSpot, int nWidth, int nHeight, byte[] pvANDPlane, byte[] pvXORPlane);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetSystemCursor(IntPtr hcur, uint id);
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);

        const uint OCR_NORMAL = 32512;
        const uint SPI_SETCURSORS = 0x0057;


        public static bool HideCursor()
        {
            Module[] list = Assembly.GetExecutingAssembly().GetModules();
            IntPtr hInst = Marshal.GetHINSTANCE(list[0]);
            byte[] pvANDPlane = { 0xff };
            byte[] pvXORPlane = { 0x00 };
            IntPtr cursor = CreateCursor(hInst, 0, 0, 1, 1, pvANDPlane, pvXORPlane);
            return SetSystemCursor(cursor, OCR_NORMAL);
        }

        public static bool ShowCursor()
        {
            return SystemParametersInfo(SPI_SETCURSORS, 0, IntPtr.Zero, 0);
        }
    }
}
