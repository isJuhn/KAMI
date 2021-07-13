using KAMI.Games;
using System;
using System.Threading;

namespace KAMI
{
    public enum KAMIStatus
    {
        Unconnected,
        Connected,
        Ready,
        Injecting,
    }

    public class KAMICore
    {
        IntPtr m_ipc;
        IGame m_game;
        MouseHandler m_mouseHandler;
        KeyHandler m_keyHandler;
        Thread m_thread;
        bool m_hideMouseCursor = false;
        bool m_closing = false;
        public bool Injecting { get; private set; } = false;
        public bool Connected { get; private set; } = false;
        public KAMIStatus Status { get; private set; } = KAMIStatus.Unconnected;
        public PineIPC.EmuStatus EmuStatus { get; private set; }
        public int? ToggleKey { get; private set; } = null;
        public int? Mouse1Key { get; private set; } = null;
        public int? Mouse2Key { get; private set; } = null;

        public delegate void UpdateHandler(object sender, IntPtr ipc);
        public event UpdateHandler OnUpdate;
        public delegate IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled);
        public HwndHook KeyboardHwndHook => m_keyHandler.HwndHook;

        public KAMICore(IntPtr windowHandle)
        {
            m_ipc = PineIPC.NewRpcs3();
            m_mouseHandler = new MouseHandler();
            m_keyHandler = new KeyHandler(windowHandle);
            m_keyHandler.OnKeyPress += (object sender) => ToggleInjector();
            m_thread = new Thread(UpdateFunction);
        }

        public void Start()
        {
            m_thread.Start();
        }

        public void Stop()
        {
            if (m_hideMouseCursor)
            {
                MouseCursor.ShowCursor();
            }
            PineIPC.DeleteRpcs3(m_ipc);
            m_keyHandler.Dispose();
            m_closing = true;
            m_thread.Join();
        }

        public void SetToggleKey(int? key)
        {
            ToggleKey = key;
            m_keyHandler.SetHotKey(KeyHandler.KeyType.InjectionToggle, ToggleKey);
        }

        public void SetMouse1Key(int? key)
        {
            Mouse1Key = key;
            m_keyHandler.SetHotKey(KeyHandler.KeyType.Mouse1, Mouse1Key);
        }

        public void SetMouse2Key(int? key)
        {
            Mouse2Key = key;
            m_keyHandler.SetHotKey(KeyHandler.KeyType.Mouse2, Mouse2Key);
        }

        public void SetSensitivity(float sensitivity)
        {
            if (m_game != null)
            {
                m_game.SensModifier = sensitivity;
            }
        }

        public void SetHideMouseCursor(bool hideMouseCursor)
        {
            m_hideMouseCursor = hideMouseCursor;
        }

        private void ToggleInjector()
        {
            if (Connected)
            {
                Injecting = !Injecting;
                if (Injecting)
                {
                    m_game.InjectionStart();
                    m_mouseHandler.GetCenterDiff();
                    m_mouseHandler.SetCursorCenter();
                    if (m_hideMouseCursor)
                    {
                        MouseCursor.HideCursor();
                    }
                }
                else if (m_hideMouseCursor)
                {
                    MouseCursor.ShowCursor();
                }
                m_keyHandler.SetEnableMouseHook(Injecting);
            }
        }

        private void CheckStatus()
        {
            if (!Connected)
            {
                Status = KAMIStatus.Unconnected;
            }
            else if (m_game == null)
            {
                Status = KAMIStatus.Connected;
            }
            else if (Injecting == false)
            {
                Status = KAMIStatus.Ready;
            }
            else
            {
                Status = KAMIStatus.Injecting;
            }
        }

        private void UpdateState()
        {
            EmuStatus = PineIPC.Status(m_ipc);
            switch (Status)
            {
                case KAMIStatus.Unconnected:
                    if (PineIPC.GetError(m_ipc) == PineIPC.IPCStatus.Success)
                    {
                        Connected = true;
                    }
                    break;
                case KAMIStatus.Connected:
                    if (PineIPC.GetError(m_ipc) != PineIPC.IPCStatus.Success)
                    {
                        Connected = false;
                        break;
                    }
                    if (EmuStatus != PineIPC.EmuStatus.Shutdown)
                    {
                        string titleId = PineIPC.GetGameID(m_ipc);
                        string gameVersion = PineIPC.GetGameVersion(m_ipc);
                        m_game = GameManager.GetGame(m_ipc, titleId, gameVersion);
                    }
                    break;
                case KAMIStatus.Ready:
                    if (PineIPC.GetError(m_ipc) != PineIPC.IPCStatus.Success)
                    {
                        Connected = false;
                        m_game = null;
                    }
                    else if (EmuStatus == PineIPC.EmuStatus.Shutdown)
                    {
                        m_game = null;
                    }
                    break;
                case KAMIStatus.Injecting:
                    if (PineIPC.GetError(m_ipc) != PineIPC.IPCStatus.Success)
                    {
                        Connected = false;
                        m_game = null;
                        Injecting = false;
                    }
                    else if (EmuStatus == PineIPC.EmuStatus.Shutdown)
                    {
                        m_game = null;
                        Injecting = false;
                    }
                    break;
            }
        }

        private void UpdateFunction()
        {
            while (!m_closing)
            {
                CheckStatus();
                UpdateState();
                OnUpdate(this, m_ipc);
                if (Status == KAMIStatus.Injecting)
                {
                    var (diffX, diffY) = m_mouseHandler.GetCenterDiff();
                    m_game.UpdateCamera(diffX, diffY);
                    m_mouseHandler.SetCursorCenter();
                }
                if (!Connected)
                {
                    Thread.Sleep(100);
                }
                else
                {
                    Thread.Sleep(8);
                }
            }
        }
    }
}
