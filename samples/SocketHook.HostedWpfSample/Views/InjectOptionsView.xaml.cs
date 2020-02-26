using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Extensions.DependencyInjection;
using SocketHook.HostedWpfSample.ViewModels;

namespace SocketHook.HostedWpfSample.Views
{
    /// <summary>
    /// Logique d'interaction pour InjectOptionsView.xaml
    /// </summary>
    public partial class InjectOptionsView : Window
    {
        public InjectOptionsView(IServiceProvider provider)
        {
            DataContext = provider.GetRequiredService<InjectOptionsViewModel>();
            Owner = Application.Current.MainWindow;
            ShowInTaskbar = false;
            InitializeComponent();
            ShowDialog();
        }

        private void WindowMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
        private void Exit_Click(object sender, RoutedEventArgs e) => Close();
    }
}
