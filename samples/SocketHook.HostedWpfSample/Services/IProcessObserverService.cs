using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SocketHook.HostedWpfSample.Controls;
using SocketHook.HostedWpfSample.Extensions;
using SocketHook.HostedWpfSample.Models;
using SocketHook.HostedWpfSample.Threading;
// ReSharper disable PossibleNullReferenceException

namespace SocketHook.HostedWpfSample.Services
{
    public interface IProcessObserverService
    {
        ReadOnlyObservableCollection<ObservedProcess> GetObservableCollection();
        bool IsCurrentlyRefreshing { get; }
        void Refresh();
    }

    internal sealed class ProcessObserverService : IProcessObserverService
    {
        private readonly TimerAwaitable _nextHeartbeat = new TimerAwaitable(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        private readonly ObservableCollection<ObservedProcess> _processes = new ObservableCollection<ObservedProcess>();
        private readonly SafeState _isCurrentlyRefreshing = new SafeState();
        private readonly object _processesMutex = new object();
        private readonly ILogger _logger;

        public bool IsCurrentlyRefreshing => _isCurrentlyRefreshing;

        public ProcessObserverService(IHostApplicationLifetime lifetime, ILogger<ProcessObserverService> logger)
        {
            BindingOperations.EnableCollectionSynchronization(_processes, _processesMutex);
            _logger = logger;

            lifetime.ApplicationStarted.Register(Start);
            lifetime.ApplicationStopping.Register(Stop);
        }

        public ReadOnlyObservableCollection<ObservedProcess> GetObservableCollection() {
            lock(_processesMutex) return new ReadOnlyObservableCollection<ObservedProcess>(_processes);
        }

        public void Refresh()
        {
            if (IsCurrentlyRefreshing) return;
            var processes = GetInjectableProcesses().ToArray();
            Application.Current.Dispatcher.Invoke(() => RefreshFrom(processes), 
                DispatcherPriority.Background);
        }

        private void Start()
        {
            _nextHeartbeat.Start();
            _ = Task.Run(ExecuteTimerLoop); // Fire and forget the timer loop
        }

        private void Stop() => _nextHeartbeat.Stop(); // Stop firing the timer

        private async Task ExecuteTimerLoop()
        {
            _logger.LogTrace("HeartBeat started.");
            Refresh(); // triggers once at startup

            using (_nextHeartbeat)
            {
                // The TimerAwaitable will return true until Stop is called
                while (await _nextHeartbeat)
                {
                    try { Refresh(); }
                    catch (Exception ex)
                    {
                        _logger.LogError($"HeartBeat failed with reason: {ex.Message}");
                        _logger.LogTrace(ex.ToString());
                    }
                }
            }
            _logger.LogTrace("HeartBeat ended.");
        }

        private void RefreshFrom(IEnumerable<ObservedProcess> processes)
        {
            if (_isCurrentlyRefreshing) return;

            lock (_processesMutex)
            {
                _isCurrentlyRefreshing.SetTrue(); // set to true when the lock is acquired
                try
                {
                    foreach (var toDelete in _processes.Where(
                        pi => processes.All(p => p.ProcessId != pi.ProcessId)).ToArray())
                        _processes.Remove(toDelete);

                    foreach (var process in processes.Where(
                        pi => _processes.All(p => p.ProcessId != pi.ProcessId)))
                        _processes.Add(process);
                }
                finally { _isCurrentlyRefreshing.SetFalse(); }
            }
        }

        private static IEnumerable<ObservedProcess> GetInjectableProcesses()
        {
            const string query = "SELECT ProcessId, Name, Priority, ExecutablePath FROM Win32_Process WHERE TerminationDate IS NULL AND ExecutablePath IS NOT NULL";
            using var searcher = new ManagementObjectSearcher(query);
            using var results = searcher.Get();

            var mo = results.Cast<ManagementObject>();
            foreach (var m in mo)
            {
                if (m == null) continue;
                var id = (uint)m["ProcessId"];
                var name = (string)m["Name"];
                var path = (string)m["ExecutablePath"];
                var priority = (uint)m["Priority"];

                yield return new ObservedProcess
                {
                    Icon = Icon.ExtractAssociatedIcon(path).ToBitmap(),
                    FileName = Path.GetFileName(path),
                    Priority = priority,
                    FilePath = path,
                    ProcessId = id,
                    Name = name
                };
            }
        }
    }
}
