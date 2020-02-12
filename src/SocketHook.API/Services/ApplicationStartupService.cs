using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SocketHook.API.Services
{
    internal sealed class ApplicationStartupService : IHostedService
    {
        private readonly HttpServerLifeTimeService _httpServer;
        private readonly IHookService _hookService;
        private readonly Settings _settings;
        private readonly ILogger _logger;

        public ApplicationStartupService(HttpServerLifeTimeService httpServer, 
            ILogger<ApplicationStartupService> logger,
            IHookService hookService, 
            Settings settings)
        {
            _hookService = hookService;
            _httpServer = httpServer;
            _settings = settings;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(_settings.HookPath) && !File.Exists(_settings.HookPath))
            {
                _logger.LogCritical($"Invalid {nameof(Settings.HookPath)} provided !");
                return;
            }

            if (_settings.ShouldInjectToExe())
            {
                if (_settings.AreInjectInputsValid()) {
                    _logger.LogCritical(
                        $"Invalid {nameof(Settings.RedirectionPort)} or {nameof(Settings.RedirectedIps)} provided !");
                    return;
                }

                try
                {
                    if (!_hookService.TryCreateAndInject(_settings.InjectToExe, _settings.RedirectionPort,
                        _settings.RedirectedIps.ToArray()))
                    {
                        _logger.LogCritical($"Couldn't find exe at {_settings.InjectToExe} !");
                        return;
                    }

                }
                catch (ArgumentException e)
                {
                    if (!string.IsNullOrWhiteSpace(e.Message)) _logger.LogError(e.Message);
                    _logger.LogCritical($"Invalid {e.ParamName} provided !");
                    return;
                }
                catch (Exception e)
                {
                    _logger.LogCritical(
                        $"An unhandled exception happened while creating and injecting at {_settings.InjectToExe} !");
                    _logger.LogDebug(e.ToString());
                    return;
                }
            }

            if (_settings.ShouldInjectToPId())
            {
                if (_settings.AreInjectInputsValid())
                {
                    _logger.LogCritical(
                        $"Invalid {nameof(Settings.RedirectionPort)} or {nameof(Settings.RedirectedIps)} provided !");
                    return;
                }

                try
                {
                    if (!_hookService.TryInject(_settings.InjectToPId, _settings.RedirectionPort,
                        _settings.RedirectedIps.ToArray()))
                    {
                        _logger.LogCritical($"Process with id={_settings.InjectToPId} has already been injected !");
                        return;
                    }

                }
                catch (ArgumentException e)
                {
                    if(!string.IsNullOrWhiteSpace(e.Message)) _logger.LogError(e.Message);
                    _logger.LogCritical($"Invalid {e.ParamName} provided !");
                    return;
                }
                catch (Exception e)
                {
                    _logger.LogCritical(
                        $"An unhandled exception happened while creating and injecting at {_settings.InjectToExe} !");
                    _logger.LogDebug(e.ToString());
                    return;
                }
            }

            await _httpServer.SetupAsync();
        }

        public async Task StopAsync(CancellationToken token)
        {
            await _httpServer.DisposeAsync();
            if(!_settings.ShouldInjectToPId() &&
               !_settings.ShouldInjectToExe())
                _hookService.KillAllInjectedProcesses();
        }
    }
}
