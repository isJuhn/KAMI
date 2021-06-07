using KAMI.Games;
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace KAMI
{
    enum KAMIStatus
    {
        Unconnected,
        Connected,
        Ready,
        Injecting,
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IntPtr m_ipc;
        IGame m_game;
        MouseHandler m_mouseHandler;
        KeyHandler m_keyHandler;
        Key? m_toggleKey = null;
        Key? m_mouse1Key = null;
        Key? m_mouse2Key = null;
        bool m_toggleButtonChange = false;
        bool m_mouse1ButtonChange = false;
        bool m_mouse2ButtonChange = false;
        bool m_injecting = false;
        bool m_closing = false;
        bool m_connected = false;
        KAMIStatus m_status = KAMIStatus.Unconnected;
        PineIPC.EmuStatus m_emuStatus;
        Thread m_thread;
        float m_sensitivity = 0.003f;

        public MainWindow()
        {
            InitializeComponent();
            m_ipc = PineIPC.NewRpcs3();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            m_mouseHandler = new MouseHandler();
            m_keyHandler = new KeyHandler(new WindowInteropHelper(this).Handle);
            m_keyHandler.OnKeyPress += (object sender) => ToggleInjector();
            m_thread = new Thread(UpdateFunction);
            m_thread.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (mouseCursorCheckBox.IsChecked ?? false)
            {
                MouseCursor.ShowCursor();
            }
            PineIPC.DeleteRpcs3(m_ipc);
            m_keyHandler.Dispose();
            m_closing = true;
            m_thread.Join();
        }

        private void toggleButton_Click(object sender, RoutedEventArgs e)
        {
            m_toggleButtonChange = true;
            toggleButton.Content = "Press key";
        }

        private void toggleButton_KeyDown(object sender, KeyEventArgs e)
        {
            if (m_toggleButtonChange)
            {
                m_toggleButtonChange = false;
                if (e.Key != Key.Escape)
                {
                    m_toggleKey = e.Key;
                }
                else
                {
                    m_toggleKey = null;
                }
                m_keyHandler.SetHotKey(KeyHandler.KeyType.InjectionToggle, m_toggleKey);
                toggleButton.Content = m_toggleKey?.ToString() ?? "Unbound";
            }
        }

        private void toggleButton_LostFocus(object sender, RoutedEventArgs e)
        {
            m_toggleButtonChange = false;
            toggleButton.Content = m_toggleKey?.ToString() ?? "Unbound";
        }

        private void mouse1Button_Click(object sender, RoutedEventArgs e)
        {
            m_mouse1ButtonChange = true;
            mouse1Button.Content = "Press key";
        }

        private void mouse1Button_KeyDown(object sender, KeyEventArgs e)
        {
            if (m_mouse1ButtonChange)
            {
                m_mouse1ButtonChange = false;
                if (e.Key != Key.Escape)
                {
                    m_mouse1Key = e.Key;
                }
                else
                {
                    m_mouse1Key = null;
                }
                m_keyHandler.SetHotKey(KeyHandler.KeyType.Mouse1, m_mouse1Key);
                mouse1Button.Content = m_mouse1Key?.ToString() ?? "Unbound";
            }
        }

        private void mouse1Button_LostFocus(object sender, RoutedEventArgs e)
        {
            m_mouse1ButtonChange = false;
            mouse1Button.Content = m_mouse1Key?.ToString() ?? "Unbound";
        }

        private void mouse2Button_Click(object sender, RoutedEventArgs e)
        {
            m_mouse2ButtonChange = true;
            mouse2Button.Content = "Press key";
        }

        private void mouse2Button_KeyDown(object sender, KeyEventArgs e)
        {
            if (m_mouse2ButtonChange)
            {
                m_mouse2ButtonChange = false;
                if (e.Key != Key.Escape)
                {
                    m_mouse2Key = e.Key;
                }
                else
                {
                    m_mouse2Key = null;
                }
                m_keyHandler.SetHotKey(KeyHandler.KeyType.Mouse2, m_mouse2Key);
                mouse2Button.Content = m_mouse2Key?.ToString() ?? "Unbound";
            }
        }

        private void mouse2Button_LostFocus(object sender, RoutedEventArgs e)
        {
            m_mouse2ButtonChange = false;
            mouse2Button.Content = m_mouse2Key?.ToString() ?? "Unbound";
        }

        private void sensitivityTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sensitivityEllipse != null && float.TryParse(sensitivityTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out float sensitivity))
            {
                m_sensitivity = sensitivity;
                if (m_game != null)
                {
                    m_game.SensModifier = sensitivity;
                }
                sensitivityEllipse.Fill = new SolidColorBrush(Color.FromRgb(0, 100, 0));
            }
            else if (sensitivityEllipse != null)
            {
                sensitivityEllipse.Fill = new SolidColorBrush(Color.FromRgb(100, 0, 0));
            }
        }

        private void ToggleInjector()
        {
            if (m_connected)
            {
                m_injecting = !m_injecting;
                if (m_injecting)
                {
                    m_game.InjectionStart();
                    m_mouseHandler.GetCenterDiff();
                    m_mouseHandler.SetCursorCenter();
                    if (mouseCursorCheckBox.IsChecked ?? false)
                    {
                        MouseCursor.HideCursor();
                    }
                }
                else if (mouseCursorCheckBox.IsChecked ?? false)
                {
                    MouseCursor.ShowCursor();
                }
                m_keyHandler.SetEnableMouseHook(m_injecting);
            }
        }

        private void CheckStatus()
        {
            if (!m_connected)
            {
                m_status = KAMIStatus.Unconnected;
            }
            else if (m_game == null)
            {
                m_status = KAMIStatus.Connected;
            }
            else if (m_injecting == false)
            {
                m_status = KAMIStatus.Ready;
            }
            else
            {
                m_status = KAMIStatus.Injecting;
            }
        }

        private void UpdateState()
        {
            m_emuStatus = PineIPC.Status(m_ipc);
            switch (m_status)
            {
                case KAMIStatus.Unconnected:
                    if (PineIPC.GetError(m_ipc) == PineIPC.IPCStatus.Success)
                    {
                        m_connected = true;
                    }
                    break;
                case KAMIStatus.Connected:
                    if (PineIPC.GetError(m_ipc) != PineIPC.IPCStatus.Success)
                    {
                        m_connected = false;
                        break;
                    }
                    if (m_emuStatus != PineIPC.EmuStatus.Shutdown)
                    {
                        string titleId = PineIPC.GetGameID(m_ipc);
                        string gameVersion = PineIPC.GetGameVersion(m_ipc);
                        m_game = GameManager.GetGame(m_ipc, titleId, gameVersion);
                    }
                    break;
                case KAMIStatus.Ready:
                    if (PineIPC.GetError(m_ipc) != PineIPC.IPCStatus.Success)
                    {
                        m_connected = false;
                        m_game = null;
                    }
                    else if (m_emuStatus == PineIPC.EmuStatus.Shutdown)
                    {
                        m_game = null;
                    }
                    break;
                case KAMIStatus.Injecting:
                    if (PineIPC.GetError(m_ipc) != PineIPC.IPCStatus.Success)
                    {
                        m_connected = false;
                        m_game = null;
                        m_injecting = false;
                    }
                    else if (m_emuStatus == PineIPC.EmuStatus.Shutdown)
                    {
                        m_game = null;
                        m_injecting = false;
                    }
                    break;
            }
        }

        private void UpdateGui()
        {
            string version = m_connected ? PineIPC.Version(m_ipc) : "";
            string title = m_connected ? PineIPC.GetGameTitle(m_ipc) : "";
            string titleId = m_connected ? PineIPC.GetGameID(m_ipc) : "";
            string gameVersion = m_connected ? PineIPC.GetGameVersion(m_ipc) : "";
            string hash = m_connected ? PineIPC.GetGameUUID(m_ipc) : "";
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (m_connected)
                {
                    infoLabel.Content = $"Version:      {version}\n";
                    infoLabel.Content += $"Title:        {title}\n";
                    infoLabel.Content += $"TitleId:      {titleId}\n";
                    infoLabel.Content += $"Game Version: {gameVersion}\n";
                    infoLabel.Content += $"Hash:         {hash}\n";
                    infoLabel.Content += $"Emu Status:   {m_emuStatus}\n";
                }
                statusLabel.Content = $"KAMI Status: {m_status}";
            }));
        }

        private void UpdateFunction()
        {
            while (!m_closing)
            {
                CheckStatus();
                UpdateState();
                UpdateGui();
                if (m_status == KAMIStatus.Injecting)
                {
                    var (diffX, diffY) = m_mouseHandler.GetCenterDiff();
                    m_game.UpdateCamera(diffX, diffY);
                    m_mouseHandler.SetCursorCenter();
                }
                if (!m_connected)
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
