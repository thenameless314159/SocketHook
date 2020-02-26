using System.Windows;
using System.Windows.Input;

namespace SocketHook.HostedWpfSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        private void WindowMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private async void Exit_Click(object sender, RoutedEventArgs e)
        { // forced to mark as async void, could also GetAwaiter().GetResult()
            await App.RunningHost.StopAsync();
            Close();
        }

        private void Maximize_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;

        private void Minimize_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState == WindowState.Normal
                ? WindowState.Minimized
                : WindowState.Normal;
    }
}
