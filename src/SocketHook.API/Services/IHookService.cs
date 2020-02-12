using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Ipc;
using EasyHook;
using Microsoft.Extensions.Logging;
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable NotAccessedField.Local

namespace SocketHook.API.Services
{
    public interface IHookService : IDisposable
    {
        bool TryCreateAndInject(string exePath, int redirectionPort, params string[] redirectedIps);
        bool TryInject(int processId, int redirectionPort, params string[] redirectedIps);
        void KillAllInjectedProcesses();
    }

    internal sealed class HookService : IHookService
    {
        private readonly ConcurrentDictionary<int, int> _registeredPid;
        private readonly IpcServerChannel _ipcServer;
        private readonly Settings _settings;
        private string _ipcChannelName;

        public HookService(ILogger<HookService> logger, Settings settings)
        {
            _ipcServer = RemoteHooking.IpcCreateServer(ref _ipcChannelName, WellKnownObjectMode.Singleton, 
                new HookInterface(logger));

            _registeredPid = new ConcurrentDictionary<int, int>();
            _settings = settings;
        }

        public bool TryCreateAndInject(string exePath, int redirectionPort, params string[] redirectedIps)
        {
            if (!File.Exists(exePath)) return false;
            if (redirectionPort == default) throw new ArgumentNullException(nameof(redirectionPort));

            RemoteHooking.CreateAndInject(
                exePath,
                string.Empty,
                0x00000004,
                InjectionOptions.DoNotRequireStrongName,
                _settings.HookPath,
                _settings.HookPath,
                out var pId,
                _ipcChannelName,
                redirectedIps,
                redirectionPort);

            _registeredPid.TryAdd(pId, pId);
            return true;
        }

        public bool TryInject(int processId, int redirectionPort, params string[] redirectedIps)
        {
            if (_registeredPid.TryGetValue(processId, out _)) return false;
            if (redirectionPort == default) throw new ArgumentNullException(nameof(redirectionPort));

            RemoteHooking.Inject(processId, InjectionOptions.DoNotRequireStrongName,
                _settings.HookPath, _settings.HookPath, _ipcChannelName,
                redirectedIps,
                redirectionPort);

            _registeredPid.TryAdd(processId, processId);
            return true;
        }

        private readonly object _mutex = new object();

        public void KillAllInjectedProcesses()
        {
            lock (_mutex) // ensure it doesn't try to kill the same processes twice
            {
                var runningProcesses = Process.GetProcesses();
                foreach (var regProcess in _registeredPid)
                    runningProcesses.FirstOrDefault(p => p.Id == regProcess.Key)?
                        .Kill();
            }
        }

        public void Dispose() { try { _ipcServer.StopListening(default); } catch { /* discarded */ } }
    }
}
