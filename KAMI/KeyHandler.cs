using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;
using System.Windows.Interop;

namespace KAMI
{
    public class KeyHandler
    {
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        const int HOTKEY_ID = 0xCA7;

        public delegate void KeyPressHandler(object sender);
        public event KeyPressHandler OnKeyPress;

        IntPtr m_hwnd;

        public KeyHandler(IntPtr hwnd)
        {
            m_hwnd = hwnd;
            HwndSource source = HwndSource.FromHwnd(m_hwnd);
            source.AddHook(HwndHook);
        }

        public void SetHotKey(Key? key)
        {
            UnregisterHotKey(m_hwnd, HOTKEY_ID);
            if (key.HasValue)
            {
                int vkey = KeyInterop.VirtualKeyFromKey(key.Value);
                RegisterHotKey(m_hwnd, HOTKEY_ID, 0, vkey);
            }
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            if (OnKeyPress != null)
                            {
                                OnKeyPress(this);
                                handled = true;
                            }
                            else
                            {
                                handled = false;
                            }
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }
    }
}
