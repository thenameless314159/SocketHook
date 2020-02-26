using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using SocketHook.Extensions.Options;

namespace SocketHook.Extensions.Services
{
    internal sealed class SocketHookHosting : IHostedService
    {
        private readonly ISocketHookService _socketHookService;
        private readonly IHookLifetimeService _hookLifetime;
        private readonly HookOptions _options;
        private readonly bool _isProduction;

        public SocketHookHosting(HookOptions options, IHookLifetimeService hookLifetime, ISocketHookService hookService, IHostEnvironment env)
        {
            _isProduction = env.IsProduction();
            _socketHookService = hookService;
            _hookLifetime = hookLifetime;
            _options = options;

        }

        public async Task StartAsync(CancellationToken token)
        {
            if(_options.OpenHookOnStartup && 
               string.IsNullOrWhiteSpace(_options.InjectToExeOnStartup))
            {
                _hookLifetime.Start(_isProduction);
                return;
            }
            // this method will triggers hook startup
            await _socketHookService.TryCreateAndInject(_options.InjectToExeOnStartup, token);
        }

        public async Task StopAsync(CancellationToken token)
        {
            if (!_hookLifetime.IsHookApplicationStarted) return;
            if (_options.KillAllOnExit)
                try { await _socketHookService.KillAllInjectedProcesses(token); }
                catch { /* discarded */ }

            _hookLifetime.Dispose();
        }
    }
}
