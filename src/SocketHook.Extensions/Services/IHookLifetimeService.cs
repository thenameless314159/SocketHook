using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SocketHook.Extensions.Services
{
    public interface IHookLifetimeService : IDisposable
    {
        bool IsHookApplicationStarted { get; }
        void Start(bool hidden = true);
    }

    internal sealed class HookLifetimeService : IHookLifetimeService
    {
        public bool IsHookApplicationStarted { get; private set; }
        
        public HookLifetimeService(string hookPath) => _hookPath = hookPath;
        private static Process _hookProcess;
        private readonly string _hookPath;

        public void Start(bool hidden = true)
        {
            const string hookName = "Socket hook API";
            if (_hookProcess != default && !_hookProcess.HasExited) return;
            var currentlyRunningHook = Process.GetProcesses()
                .FirstOrDefault(process => process.MainWindowTitle == hookName);

            if (currentlyRunningHook != null && !currentlyRunningHook.HasExited)
            {
                _hookProcess = currentlyRunningHook;
                IsHookApplicationStarted = true;
                return;
            }

            var infos = new ProcessStartInfo(_hookPath) { WorkingDirectory = Path.GetDirectoryName(_hookPath) };
            if (!hidden) infos.Arguments = "--environment=development";
            else
            {
                infos.WindowStyle = ProcessWindowStyle.Hidden;
                infos.Arguments = "--environment=Production";
                infos.CreateNoWindow = true;
            }

            _hookProcess = Process.Start(infos);
            IsHookApplicationStarted = true;
        }

        public void Dispose()
        {
            if (!IsHookApplicationStarted) return;
            try { IsHookApplicationStarted = false; _hookProcess?.Kill();  }
            catch { /* discarded */ }
        }
    }
}
