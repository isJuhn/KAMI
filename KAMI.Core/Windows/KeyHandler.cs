#if Windows
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using KAMI.Core.Common;

namespace KAMI.Core
{
    public delegate IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled);

    public class KeyHandler : IKeyHandler, IDisposable
    {
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc callback, IntPtr hInstance, uint threadId);
        private delegate IntPtr LowLevelMouseProc(int nCode, UIntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        [DllImport("user32.dll")]
        static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, UIntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, uint uMapType);

        const int WM_HOTKEY = 0x0312;
        const int WM_LBUTTONDOWN = 0x0201;
        const int WM_LBUTTONUP = 0x0202;
        const int WM_RBUTTONDOWN = 0x0204;
        const int WM_RBUTTONUP = 0x0205;
        const int WH_MOUSE_LL = 14;

        public event KeyPressHandler OnKeyPress;

        IntPtr m_hwnd;
        IntPtr m_llhook = IntPtr.Zero;
        bool m_mouseHook = false;
        LowLevelMouseProc m_mouseProc;
        int? m_mouse1 = null;
        int? m_mouse2 = null;


        public KeyHandler(IntPtr hwnd, Action<HwndHook> addHookAction)
        {
            m_hwnd = hwnd;
            m_mouseProc = LowLevelHookProc;
            addHookAction(HwndHook);
        }

        public void SetHotKey(KeyType keyType, int? key)
        {
            switch (keyType)
            {
                case KeyType.InjectionToggle:
                    UnregisterHotKey(m_hwnd, (int)keyType);
                    if (key.HasValue)
                    {
                        RegisterHotKey(m_hwnd, (int)keyType, 0, key.Value);
                    }
                    break;
                case KeyType.Mouse1:
                    m_mouse1 = key.HasValue ? key.Value : null;
                    break;
                case KeyType.Mouse2:
                    m_mouse2 = key.HasValue ? key.Value : null;
                    break;
            }
        }

        public void SetEnableMouseHook(bool enabled)
        {
            m_mouseHook = enabled;
            if (m_mouseHook && (m_mouse1.HasValue || m_mouse2.HasValue))
            {
                Module[] list = Assembly.GetExecutingAssembly().GetModules();
                m_llhook = SetWindowsHookEx(WH_MOUSE_LL, m_mouseProc, Marshal.GetHINSTANCE(list[0]), 0);
            }
            else if (m_llhook != IntPtr.Zero)
            {
                UnhookWindowsHookEx(m_llhook);
                m_llhook = IntPtr.Zero;
            }
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case (int)KeyType.InjectionToggle:
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

        private IntPtr LowLevelHookProc(int code, UIntPtr wParam, IntPtr lParam)
        {
            bool handled = false;
            if (code >= 0 && m_mouseHook)
            {
                uint button = wParam.ToUInt32();
                switch (button)
                {
                    case WM_LBUTTONDOWN:
                        if (m_mouse1.HasValue)
                        {
                            SendKey(m_mouse1.Value);
                            handled = true;
                        }
                        break;
                    case WM_RBUTTONDOWN:
                        if (m_mouse2.HasValue)
                        {
                            SendKey(m_mouse2.Value);
                            handled = true;
                        }
                        break;
                    case WM_LBUTTONUP:
                        if (m_mouse1.HasValue)
                        {
                            SendKey(m_mouse1.Value, false);
                            handled = true;
                        }
                        break;
                    case WM_RBUTTONUP:
                        if (m_mouse2.HasValue)
                        {
                            SendKey(m_mouse2.Value, false);
                            handled = true;
                        }
                        break;
                }
            }
            if (!handled)
            {
                return CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
            }
            return (IntPtr)1;
        }

        private void SendKey(int keyCode, bool down = true)
        {
            var input = new INPUT
            {
                type = (uint)InputType.INPUT_KEYBOARD,
                keyboardInput = new KEYBDINPUT
                {
                    wVk = (ushort)keyCode,
                    wScan = (ushort)(MapVirtualKey((uint)m_mouse2.Value, 0) & 0xFFU),
                    dwFlags = down ? 0u : (uint)InputFlags.KEYEVENTF_KEYUP,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            };
            SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }

        public void Dispose()
        {
            UnregisterHotKey(m_hwnd, (int)KeyType.InjectionToggle);
            if (m_llhook != IntPtr.Zero)
            {
                UnhookWindowsHookEx(m_llhook);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x28)]
    internal struct INPUT
    {
        internal uint type;
        internal KEYBDINPUT keyboardInput;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct KEYBDINPUT
    {
        internal ushort wVk;
        internal ushort wScan;
        internal uint dwFlags;
        internal uint time;
        internal IntPtr dwExtraInfo;
    }

    internal enum InputType
    {
        INPUT_MOUSE    = 0,
        INPUT_KEYBOARD = 1,
        INPUT_HARDWARE = 2,
    }

    internal enum InputFlags
    {
        KEYEVENTF_EXTENDEDKEY = 1,
        KEYEVENTF_KEYUP       = 2,
        KEYEVENTF_UNICODE     = 4,
        KEYEVENTF_SCANCODE    = 8,
    }
}
#endif
