using System;
using Microsoft.Extensions.Logging;

namespace SocketHook
{
    public class HookInterface : MarshalByRefObject
    {
        public HookInterface(ILogger logger) => _logger = logger;
        private readonly ILogger _logger;

        public void NotifyInstalled(string processName, int pid) => _logger?.LogInformation($"Successfully injected to {processName}.exe with pid={pid} !");
        public void LogInformation(string message) => _logger?.LogInformation(message);
        public void LogWarning(string message) => _logger?.LogWarning(message);
        public void LogDebug(string message) => _logger?.LogDebug(message);

        public void OnError(Exception ex)
        {
            _logger?.LogError($"An unhandled exception happened with reason : {ex.Message}");
            _logger?.LogDebug(ex.ToString());
        }
    }
}
