using KAMI.Core.Common;
using KAMI.Core.Games;
using KAMI.Core.Utilities;
using KAMI.Core.Windows;
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
        IMouseHandler m_mouseHandler;
        IKeyHandler m_keyHandler;
        Thread m_thread;
        ConfigManager<KamiConfig> m_configManager;
        GameManager m_gameManager;
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
        IntPtr windowHandle;
        Action<HwndHook> addHookAction;
        public KAMICore(IntPtr windowHandle, Action<HwndHook> addHookAction, Action<Exception> exceptionCallback)
        {
            this.windowHandle = windowHandle;
            this.addHookAction = addHookAction;
            m_configManager = new ConfigManager<KamiConfig>("config.json");
            m_gameManager = new GameManager();
            m_keyHandler = new KeyHandler(windowHandle, addHookAction);
            m_keyHandler.OnKeyPress += (object sender) => ToggleInjector();
            m_thread = new Thread(UpdateFunction);
            m_exceptionCallback = exceptionCallback;
            ReloadConfig();
            m_ipc = Config.UsePCSX2 ? PineIPC.NewPcsx2() : PineIPC.NewRpcs3();
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
            if (Config.UsePCSX2)
            {
                PineIPC.DeletePcsx2(m_ipc);
            }
            else
            {
                PineIPC.DeleteRpcs3(m_ipc);
            }

            if (Config.HideCursor)
            {
                MouseCursor.ShowCursor();
            }
            m_mouseHandler.ReleaseCursor();
            m_keyHandler.Dispose();
            m_closing = true;
            m_thread.Join();
        }

        public void ReloadConfig()
        {
            m_configManager.ReloadConfig();
            m_keyHandler.SetHotKey(KeyType.InjectionToggle, Config.ToggleKey);
            m_keyHandler.SetHotKey(KeyType.Mouse1, Config.Mouse1Key);
            m_keyHandler.SetHotKey(KeyType.Mouse2, Config.Mouse2Key);
            if (m_game != null)
            {
                m_game.SensModifier = Config.Sensitivity;
            }

            m_mouseHandler = Config.MouseHandler switch
            {
                MouseHandlerEnum.Cursor => new CursorMouseHandler(),
#if Windows
                MouseHandlerEnum.RawInput => new RawInputMouseHandler(windowHandle, addHookAction),
#endif
                _ => throw new Exception("Unknown mouse handler"),
            };
        }

        public void SetToggleKey(int? key)
        {
            Config.ToggleKey = key;
            m_configManager.WriteConfig();
            m_keyHandler.SetHotKey(KeyType.InjectionToggle, key);
        }

        public void SetMouse1Key(int? key)
        {
            Config.Mouse1Key = key;
            m_configManager.WriteConfig();
            m_keyHandler.SetHotKey(KeyType.Mouse1, key);
        }

        public void SetMouse2Key(int? key)
        {
            Config.Mouse2Key = key;
            m_configManager.WriteConfig();
            m_keyHandler.SetHotKey(KeyType.Mouse2, key);
        }

        public void SetSensitivity(float sensitivity)
        {
            Config.Sensitivity = sensitivity;
            m_configManager.WriteConfig();
            if (m_game != null)
            {
                m_game.SensModifier = sensitivity;
            }
        }

        public void SetHideMouseCursor(bool hideMouseCursor)
        {
            Config.HideCursor = hideMouseCursor;
            m_configManager.WriteConfig();
        }

        public void SetEmulator(bool pcsx2)
        {
            if (Config.UsePCSX2 == pcsx2) return;

            bool started = m_thread?.IsAlive == true;

            if (started) Stop();
            Config.UsePCSX2 = pcsx2;
            m_configManager.WriteConfig();
            m_ipc = Config.UsePCSX2 ? PineIPC.NewPcsx2() : PineIPC.NewRpcs3();
            m_thread = new Thread(UpdateFunction);
            m_closing = false;
            if (started)
            {
                m_keyHandler = new KeyHandler(windowHandle, addHookAction);
                m_keyHandler.OnKeyPress += (object sender) => ToggleInjector();
                Start();
            }
            ReloadConfig();
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
                    m_mouseHandler.ConfineCursor();
                    if (Config.HideCursor)
                    {
                        MouseCursor.HideCursor();
                    }
                }
                else if (Config.HideCursor)
                {
                    MouseCursor.ShowCursor();
                }
                m_keyHandler.SetEnableMouseHook(Injecting);
            }
            if (!Injecting)
            {
                m_mouseHandler.ReleaseCursor();
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
                        m_game = m_gameManager.GetGame(m_ipc, titleId, gameVersion);
                        m_game.SensModifier = Config.Sensitivity;
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
                    else
                    {
                        string titleId = PineIPC.GetGameID(m_ipc);
                        string gameVersion = PineIPC.GetGameVersion(m_ipc);
                        if (m_gameManager.CurrentGameId != titleId || m_gameManager.CurrentGameVersion != gameVersion)
                        {
                            m_game = m_gameManager.GetGame(m_ipc, titleId, gameVersion);
                            m_game.SensModifier = Config.Sensitivity;
                        }
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
