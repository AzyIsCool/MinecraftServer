using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace MinecraftServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        public MainWindow()
        {
            InitializeComponent();
            PageToShow.Content = new MainPage(this);
        }

        private void RecHandle_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                ReleaseCapture();
                SendMessage(hwnd, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                hwnd = new IntPtr(0);
            }
        }

        private void Close_OnMouseEnter(object sender, MouseEventArgs e)
        {
            CloseIcon.Fill = Brushes.White;
        }

        private void Close_OnMouseLeave(object sender, MouseEventArgs e)
        {
            CloseIcon.Fill = (Brush)App.Current.Resources["TextColourSCBrush"];
        }

        private void Min_OnClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MainWindow_OnStateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
                WindowsContent.Margin = new Thickness(7);
            else
                WindowsContent.Margin = new Thickness(0);
        }

        private void MainWindow_OnActivated(object sender, EventArgs e)
        {
            WindowsContent.BorderThickness = new Thickness(1);
        }

        private void MainWindow_OnDeactivated(object sender, EventArgs e)
        {
            WindowsContent.BorderThickness = new Thickness(0);
        }
    }
}
