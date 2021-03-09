using KAMI.Games;
using System;
using System.Collections.Generic;
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
        Thread m_thread;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            m_ipc = PCSX2IPC.New();
            statusLabel.Content = "Kami Status: Connected";
            infoLabel.Content = $"Info: {PCSX2IPC.Version(m_ipc)}\n{PCSX2IPC.GetGameTitle(m_ipc)}\n{PCSX2IPC.GetGameID(m_ipc)}\n{PCSX2IPC.GetGameUUID(m_ipc)}";
            m_game = GameManager.GetGame(m_ipc, PCSX2IPC.GetGameID(m_ipc));
            m_mouseHandler = new MouseHandler();
            m_keyHandler = new KeyHandler(new WindowInteropHelper(this).Handle);
            m_keyHandler.OnKeyPress += (object sender) => ToggleInjector();
            m_thread = new Thread(InjectorFunction);
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

        private void ToggleInjector()
        {
            m_injecting = !m_injecting;
            if (m_injecting)
            {
                statusLabel.Content = "Kami Status: Injecting";
                m_game.InjectionStart();
                m_mouseHandler.GetCenterDiff();
                m_mouseHandler.SetCursorCenter();
            } else
            {
                statusLabel.Content = "Kami Status: Connected";
            }
        }

        private void InjectorFunction()
        {
            while (!m_closing)
            {
                if (m_injecting)
                {
                    var (diffX, diffY) = m_mouseHandler.GetCenterDiff();
                    m_game.UpdateCamera(diffX, diffY);
                    m_mouseHandler.SetCursorCenter();
                }
                Thread.Sleep(8);
            }
        }
    }
}
