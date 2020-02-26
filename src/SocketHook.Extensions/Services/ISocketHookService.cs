using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SocketHook.Extensions.Options;

namespace SocketHook.Extensions.Services
{
    public interface ISocketHookService
    {
        ValueTask<bool> TryCreateAndInject(string exePath, CancellationToken token = default);
        ValueTask<bool> TryInject(int pId, CancellationToken token = default);

        /// <summary>
        /// Kill all the processes that might have been created using this service.
        /// </summary>
        /// <exception cref="HttpRequestException" />
        ValueTask KillAllInjectedProcesses(CancellationToken token = default);
    }

    internal sealed class SocketHookService : ISocketHookService
    {
        private const string _createAndInjectRoute = "createandinject?exePath=";
        private const string _mediaType = "application/json";
        private const string _injectRoute = "inject?pId=";
        private const string _killAllRoute = "killall";

        private readonly IHookLifetimeService _hookLifetime;
        private readonly string _injectionSettings;
        private readonly HttpClient _client;

        public SocketHookService(HttpClient client, IHookLifetimeService hookLifetime, InjectOptions options = default)
        {
            _injectionSettings = options?.ToString();
            _hookLifetime = hookLifetime;
            _client = client;
        }

        public async ValueTask<bool> TryCreateAndInject(string exePath, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(exePath)) return false;
            if (!_hookLifetime.IsHookApplicationStarted) _hookLifetime.Start();
            var response = await _client.PutAsync(_createAndInjectRoute + exePath,
                new StringContent(_injectionSettings, Encoding.UTF8, _mediaType), token);

            return response.IsSuccessStatusCode;
        }

        public async ValueTask<bool> TryInject(int pId, CancellationToken token = default)
        {
            if (pId == default) return false;
            if (!_hookLifetime.IsHookApplicationStarted) _hookLifetime.Start();
            var response = await _client.PutAsync(_injectRoute + pId,
                new StringContent(_injectionSettings, Encoding.UTF8, _mediaType), token);

            return response.IsSuccessStatusCode;
        }

        /// <inheritdoc cref="ISocketHookService.KillAllInjectedProcesses"/>
        public async ValueTask KillAllInjectedProcesses(CancellationToken token = default)
        {
            if (!_hookLifetime.IsHookApplicationStarted) return;
            var response = await _client.DeleteAsync(_killAllRoute, token);
            response.EnsureSuccessStatusCode();
        }
    }
}
