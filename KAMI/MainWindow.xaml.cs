using KAMI.Games;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        Key? m_key = null;
        bool m_buttonChange = false;
        bool m_injecting = false;
        bool m_closing = false;
        bool m_connected = false;
        KAMIStatus m_status = KAMIStatus.Unconnected;
        PCSX2IPC.EmuStatus m_emuStatus;
        Thread m_thread;
        float m_sensitivity = 0.003f;

        public MainWindow()
        {
            InitializeComponent();
            m_ipc = PCSX2IPC.New();
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
            PCSX2IPC.Delete(m_ipc);
            m_closing = true;
            m_thread.Join();
        }

        private void toggleButton_Click(object sender, RoutedEventArgs e)
        {
            m_buttonChange = true;
            toggleButton.Content = "Press key";
        }

        private void toggleButton_KeyDown(object sender, KeyEventArgs e)
        {
            if (m_buttonChange)
            {
                m_buttonChange = false;
                if (e.Key != Key.Escape)
                {
                    m_key = e.Key;
                }
                else
                {
                    m_key = null;
                }
                m_keyHandler.SetHotKey(m_key);
                toggleButton.Content = m_key?.ToString() ?? "Unbound";
            }
        }

        private void toggleButton_LostFocus(object sender, RoutedEventArgs e)
        {
            m_buttonChange = false;
            toggleButton.Content = m_key?.ToString() ?? "Unbound";
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
                }
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
            m_emuStatus = PCSX2IPC.Status(m_ipc);
            switch (m_status)
            {
                case KAMIStatus.Unconnected:
                    if (PCSX2IPC.GetError(m_ipc) == PCSX2IPC.IPCStatus.Success)
                    {
                        m_connected = true;
                    }
                    break;
                case KAMIStatus.Connected:
                    if (PCSX2IPC.GetError(m_ipc) != PCSX2IPC.IPCStatus.Success)
                    {
                        m_connected = false;
                        break;
                    }
                    if (m_emuStatus != PCSX2IPC.EmuStatus.Shutdown)
                    {
                        string titleId = PCSX2IPC.GetGameID(m_ipc);
                        string gameVersion = PCSX2IPC.GetGameVersion(m_ipc);
                        m_game = GameManager.GetGame(m_ipc, titleId, gameVersion);
                    }
                    break;
                case KAMIStatus.Ready:
                    if (PCSX2IPC.GetError(m_ipc) != PCSX2IPC.IPCStatus.Success)
                    {
                        m_connected = false;
                        m_game = null;
                    }
                    else if (m_emuStatus == PCSX2IPC.EmuStatus.Shutdown)
                    {
                        m_game = null;
                    }
                    break;
                case KAMIStatus.Injecting:
                    if (PCSX2IPC.GetError(m_ipc) != PCSX2IPC.IPCStatus.Success)
                    {
                        m_connected = false;
                        m_game = null;
                        m_injecting = false;
                    }
                    else if (m_emuStatus == PCSX2IPC.EmuStatus.Shutdown)
                    {
                        m_game = null;
                        m_injecting = false;
                    }
                    break;
            }
        }

        private void UpdateGui()
        {
            string version = m_connected ? PCSX2IPC.Version(m_ipc) : "";
            string title = m_connected ? PCSX2IPC.GetGameTitle(m_ipc) : "";
            string titleId = m_connected ? PCSX2IPC.GetGameID(m_ipc) : "";
            string gameVersion = m_connected ? PCSX2IPC.GetGameVersion(m_ipc) : "";
            string hash = m_connected ? PCSX2IPC.GetGameUUID(m_ipc) : "";
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
