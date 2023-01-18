using KAMI.Core.Games;
using KAMI.Core.Utilities;
using System;
using System.Threading;

namespace KAMI.Core
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
        IKeyHandler m_keyHandler;
        Thread m_thread;
        ConfigManager<KamiConfig> m_configManager;
        Action<Exception> m_exceptionCallback;
        bool m_closing = false;
        public KamiConfig Config => m_configManager.Config;
        public bool Injecting { get; private set; } = false;
        public bool Connected { get; private set; } = false;
        public KAMIStatus Status { get; private set; } = KAMIStatus.Unconnected;
        public PineIPC.EmuStatus EmuStatus { get; private set; }

        public delegate void UpdateHandler(object sender, IntPtr ipc);
        public event UpdateHandler OnUpdate;

#if Windows
        public KAMICore(IntPtr windowHandle, Action<HwndHook> addHookAction, Action<Exception> exceptionCallback)
        {
            m_configManager = new ConfigManager<KamiConfig>("config.json");
            m_ipc = PineIPC.NewRpcs3();
            m_mouseHandler = new MouseHandler();
            m_keyHandler = new KeyHandler(windowHandle, addHookAction);
            m_keyHandler.OnKeyPress += (object sender) => ToggleInjector();
            m_thread = new Thread(UpdateFunction);
            m_exceptionCallback = exceptionCallback;
        }

#elif Linux
        public KAMICore()
        {
            m_config = new ConfigManager<KamiConfig>("~/.config/rpcs3/config.json");
            m_ipc = PineIPC.NewRpcs3();
            m_mouseHandler = new MouseHandler();
            m_keyHandler = new KeyHandler();
            m_keyHandler.OnKeyPress += (object sender) => ToggleInjector();
            m_thread = new Thread(UpdateFunction);
        }
#endif

        public void Start()
        {
            m_thread.Start();
        }

        public void Stop()
        {
            if (m_configManager.Config.HideCursor)
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
            m_configManager.Config.ToggleKey = key;
            m_configManager.WriteConfig();
            m_keyHandler.SetHotKey(KeyType.InjectionToggle, key);
        }

        public void SetMouse1Key(int? key)
        {
            m_configManager.Config.Mouse1Key = key;
            m_configManager.WriteConfig();
            m_keyHandler.SetHotKey(KeyType.Mouse1, key);
        }

        public void SetMouse2Key(int? key)
        {
            m_configManager.Config.Mouse2Key = key;
            m_configManager.WriteConfig();
            m_keyHandler.SetHotKey(KeyType.Mouse2, key);
        }

        public void SetSensitivity(float sensitivity)
        {
            m_configManager.Config.Sensitivity = sensitivity;
            m_configManager.WriteConfig();
            if (m_game != null)
            {
                m_game.SensModifier = sensitivity;
            }
        }

        public void SetHideMouseCursor(bool hideMouseCursor)
        {
            m_configManager.Config.HideCursor = hideMouseCursor;
            m_configManager.WriteConfig();
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
                    if (m_configManager.Config.HideCursor)
                    {
                        MouseCursor.HideCursor();
                    }
                }
                else if (m_configManager.Config.HideCursor)
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
                        m_game.SensModifier = m_configManager.Config.Sensitivity;
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
                try
                {
                    CheckStatus();
                    UpdateState();
                    if (OnUpdate != null)
                    {
                        OnUpdate(this, m_ipc);
                    }
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
                catch (Exception ex)
                {
                    if (m_exceptionCallback != null)
                    {
                        m_exceptionCallback(ex);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }
}
