using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using SocketHook.Extensions.Options;
using SocketHook.HostedWpfSample.Commands;
using SocketHook.HostedWpfSample.Services;

namespace SocketHook.HostedWpfSample.ViewModels
{
    public class InjectOptionsViewModel : ViewModelBase
    {
        private string _settingsFilePath;
        public string SettingsFilePath
        {
            get => _settingsFilePath;
            set => Set(ref _settingsFilePath, value, nameof(SettingsFilePath));
        }

        private string _redirectedIps;
        public string RedirectedIps
        {
            get => _redirectedIps;
            set => Set(ref _redirectedIps, value, nameof(RedirectedIps));
        }

        private string _redirectionPort;
        public string RedirectionPort
        {
            get => _redirectionPort;
            set => Set(ref _redirectionPort, value, nameof(RedirectionPort));
        }
        
        private bool _isPortValid;
        public bool IsPortValid
        {
            get => _isPortValid;
            set => Set(ref _isPortValid, value, nameof(IsPortValid));
        }
        
        private bool _isPortNotValid;
        public bool IsPortNotValid
        {
            get => _isPortNotValid;
            set => Set(ref _isPortNotValid, value, nameof(IsPortNotValid));
        }

        private (InjectOptions Options, string Path) _current;

        public InjectOptionsViewModel(IInjectOptionsService optionsService)
        {
            Save = ActionCommand.Create<string>(settingsPath =>
            {
                if (string.IsNullOrWhiteSpace(RedirectionPort) || !int.TryParse(RedirectionPort, out var port))
                {
                    IsPortNotValid = true;
                    IsPortValid = false;
                    return;
                }
                if (string.IsNullOrWhiteSpace(RedirectedIps))
                {
                    MessageBox.Show("Invalid redirected Ips !", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (!IPAddress.TryParse(RedirectedIps, out _) && !RedirectedIps.Contains(','))
                {
                    MessageBox.Show("Invalid redirected Ips !", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var redirectedIps = RedirectedIps.Split(',');
                if(redirectedIps.All(ip => !IPAddress.TryParse(ip, out _)))
                {
                    MessageBox.Show("Invalid redirected Ips !", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (_current.Options.RedirectionPort == port &&
                    _current.Options.RedirectedIps.SequenceEqual(redirectedIps))
                    return; // doesn't need to save nor telling to the user

                if (!optionsService.TrySaveAsJson(settingsPath, new InjectOptions(port, redirectedIps)))
                {
                    MessageBox.Show("Couldn't save inject options !", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                MessageBox.Show($"Options has successfully been saved at : {settingsPath} !", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            });
            Load = ActionCommand.Create<string>(settingsPath =>
            {
                if (_current.Path == settingsPath) return;
                if (!optionsService.TryLoadFromJson(settingsPath, out var options))
                {
                    MessageBox.Show($"Couldn't load inject options from : {settingsPath} !", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (options.RedirectedIps.All(ip => !IPAddress.TryParse(ip, out _)))
                {
                    MessageBox.Show("Loaded redirected Ips are invalid !", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                _current = (options, settingsPath);
                UpdatePortStatus(options.RedirectionPort);
                RedirectionPort = options.RedirectionPort.ToString();
                RedirectedIps = string.Join(',', options.RedirectedIps);
                MessageBox.Show($"Options has successfully been loaded from : {settingsPath} !", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            });
            SelectFileFromExplorer = ActionCommand.Create(() =>
            {
                var dialog = new OpenFileDialog
                {
                    InitialDirectory = Directory.GetCurrentDirectory(),
                    Filter = "JSON Files(*.json) | *.json",
                    DefaultExt = ".json",
                    Multiselect = false
                };

                var result = dialog.ShowDialog();
                if (result.HasValue && result == true) SettingsFilePath = dialog.FileName;
            });
            _current = optionsService.GetCurrentOptions();
            if (_current == default)
            {
                SettingsFilePath = "couldn't load from configuration !";
                IsPortNotValid = true;
                IsPortValid = false;
                return;
            }

            RedirectedIps = string.Join(',', _current.Options.RedirectedIps);
            RedirectionPort = _current.Options.RedirectionPort.ToString();
            SettingsFilePath = Path.GetFullPath(_current.Path);
            UpdatePortStatus(_current.Options.RedirectionPort);
        }

        public ICommand Save { get; }
        public ICommand Load { get; }
        public ICommand SelectFileFromExplorer { get; }

        private void UpdatePortStatus(int port)
        {
            try
            {
                using var client = new TcpClient();
                client.Connect(IPAddress.Loopback, port);
                IsPortNotValid = !client.Connected;
                IsPortValid = client.Connected;
            }
            catch (SocketException)
            {
                IsPortNotValid = true;
                IsPortValid = false;
            }
        }
    }
}
