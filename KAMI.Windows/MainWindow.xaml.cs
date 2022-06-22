using KAMI.Core;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace KAMI.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KAMICore m_kami;
        bool m_toggleButtonChange = false;
        bool m_mouse1ButtonChange = false;
        bool m_mouse2ButtonChange = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            HwndSource source = HwndSource.FromHwnd(hwnd);
            m_kami = new KAMICore(hwnd, (hwndHook) => source.AddHook(new HwndSourceHook(hwndHook)));
            m_kami.OnUpdate += (object sender, IntPtr ipc) => UpdateGui(ipc);

            toggleButton.Content = FromVKey(m_kami.Config.ToggleKey)?.ToString() ?? "Unbound";
            mouse1Button.Content = FromVKey(m_kami.Config.Mouse1Key)?.ToString() ?? "Unbound";
            mouse2Button.Content = FromVKey(m_kami.Config.Mouse2Key)?.ToString() ?? "Unbound";
            sensitivityTextBox.Text = m_kami.Config.Sensitivity.ToString(CultureInfo.InvariantCulture);
            mouseCursorCheckBox.IsChecked = m_kami.Config.HideCursor;

            m_kami.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_kami.Stop();
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
                Key? key = e.Key != Key.Escape ? e.Key : null;
                m_kami.SetToggleKey(ToVKey(key));
                toggleButton.Content = key?.ToString() ?? "Unbound";
            }
        }

        private void toggleButton_LostFocus(object sender, RoutedEventArgs e)
        {
            m_toggleButtonChange = false;
            toggleButton.Content = FromVKey(m_kami.Config.ToggleKey)?.ToString() ?? "Unbound";
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
                Key? key = e.Key != Key.Escape ? e.Key : null;
                m_kami.SetMouse1Key(ToVKey(key));
                mouse1Button.Content = key?.ToString() ?? "Unbound";
            }
        }

        private void mouse1Button_LostFocus(object sender, RoutedEventArgs e)
        {
            m_mouse1ButtonChange = false;
            mouse1Button.Content = FromVKey(m_kami.Config.Mouse1Key)?.ToString() ?? "Unbound";
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
                Key? key = e.Key != Key.Escape ? e.Key : null;
                m_kami.SetMouse2Key(ToVKey(key));
                mouse2Button.Content = key?.ToString() ?? "Unbound";
            }
        }

        private void mouse2Button_LostFocus(object sender, RoutedEventArgs e)
        {
            m_mouse2ButtonChange = false;
            mouse2Button.Content = FromVKey(m_kami.Config.Mouse2Key)?.ToString() ?? "Unbound";
        }

        private void sensitivityTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sensitivityEllipse != null && float.TryParse(sensitivityTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out float sensitivity))
            {
                m_kami.SetSensitivity(sensitivity);
                sensitivityEllipse.Fill = new SolidColorBrush(Color.FromRgb(0, 100, 0));
            }
            else if (sensitivityEllipse != null)
            {
                sensitivityEllipse.Fill = new SolidColorBrush(Color.FromRgb(100, 0, 0));
            }
        }

        private void UpdateGui(IntPtr ipc)
        {
            string version = m_kami.Connected ? PineIPC.Version(ipc) : "";
            string title = m_kami.Connected ? PineIPC.GetGameTitle(ipc) : "";
            string titleId = m_kami.Connected ? PineIPC.GetGameID(ipc) : "";
            string gameVersion = m_kami.Connected ? PineIPC.GetGameVersion(ipc) : "";
            string hash = m_kami.Connected ? PineIPC.GetGameUUID(ipc) : "";
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (m_kami.Connected)
                {
                    infoLabel.Content  = $"Version:      {version}\n";
                    infoLabel.Content += $"Title:        {title}\n";
                    infoLabel.Content += $"TitleId:      {titleId}\n";
                    infoLabel.Content += $"Game Version: {gameVersion}\n";
                    infoLabel.Content += $"Hash:         {hash}\n";
                    infoLabel.Content += $"Emu Status:   {m_kami.EmuStatus}\n";
                }
                statusLabel.Content = $"KAMI Status: {m_kami.Status}";
            }));
        }

        private void mouseCursorCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            m_kami.SetHideMouseCursor(true);
        }

        private void mouseCursorCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            m_kami.SetHideMouseCursor(false);
        }

        private int? ToVKey(Key? key)
        {
            return key != null ? KeyInterop.VirtualKeyFromKey(key.Value) : null;
        }

        private Key? FromVKey(int? key)
        {
            return key != null ? KeyInterop.KeyFromVirtualKey(key.Value) : null;
        }
    }
}
