using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using SocketHook.Extensions.Services;
using SocketHook.HostedWpfSample.Commands;
using SocketHook.HostedWpfSample.Models;
using SocketHook.HostedWpfSample.Services;
using SocketHook.HostedWpfSample.ViewModels;
using SocketHook.HostedWpfSample.Views;
// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

namespace SocketHook.HostedWpfSample
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ReadOnlyObservableCollection<ObservedProcess> Processes { get; }

        public MainWindowViewModel(IServiceProvider provider)
        {
            var processObserver = provider.GetRequiredService<IProcessObserverService>();
            _injectorFactory = provider.GetRequiredService<ISocketHookServiceFactory>();
            _optionsService = provider.GetRequiredService<IInjectOptionsService>();
            Processes = processObserver.GetObservableCollection();
            
            Refresh = ActionCommand.Create(processObserver.Refresh);
            Settings = ActionCommand.Create(() => provider.GetRequiredService<InjectOptionsView>());
            InjectTo = AsyncCommand.CreateWithInput<ObservedProcess>(async item =>
            {
                if (item == default)
                {
                    MessageBox.Show("You must select a process before injecting !", "Warning", MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                    return;
                }

                var current = _optionsService.GetCurrentOptions();
                if (current == default)
                {
                    MessageBox.Show("Couldn't find inject options, you must configure them within the settings view !",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var (options, _) = current;
                var injector = _injectorFactory.CreateWith(options);
                if (!await injector.TryInject((int)item.ProcessId))
                {
                    MessageBox.Show($"Couldn't inject to process with id:{item.ProcessId}, name:{item.FileName} !", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                MessageBox.Show($"Successfully injected to current {item.FileName} process !", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            });

            CreateAndInject = AsyncCommand.Create(async () =>
            {
                var dialog = new OpenFileDialog
                {
                    InitialDirectory = Directory.GetCurrentDirectory(),
                    Filter = "EXE Files(*.exe) | *.exe",
                    DefaultExt = ".exe",
                    Multiselect = false
                };

                var result = dialog.ShowDialog();
                if (!result.HasValue || result == false) return;
                var current = _optionsService.GetCurrentOptions();
                if (current == default)
                {
                    MessageBox.Show("Couldn't find inject options, you must configure them within the settings view !",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var (options, _) = current;
                var injector = _injectorFactory.CreateWith(options);
                if (!await injector.TryCreateAndInject(dialog.FileName))
                {
                    MessageBox.Show($"Couldn't create and inject to file at : {dialog.FileName} !", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                MessageBox.Show($"Successfully created and injected executable at {dialog.FileName} !", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        private readonly ISocketHookServiceFactory _injectorFactory;
        private readonly IInjectOptionsService _optionsService;

        public ICommand CreateAndInject { get; }
        public ICommand InjectTo { get; }
        public ICommand Settings { get; }
        public ICommand Refresh { get; }
    }
}
