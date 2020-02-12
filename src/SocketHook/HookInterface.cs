using System;
using Microsoft.Extensions.Logging;

namespace SocketHook
{
    public class HookInterface : MarshalByRefObject
    {
        private readonly ILogger _logger;
        private int _count;

        public HookInterface(ILogger logger) => _logger = logger;

        public void NotifyInstalled(string processName, int pid) => _logger.LogInformation($"Successfully injected to {processName}.exe with pid={pid} !");

        public void Message(string message) => _logger.LogInformation(message);

        public void OnError(Exception ex)
        {
            _logger.LogError($"An unhandled exception happened with reason : {ex.Message}");
            _logger.LogDebug(ex.ToString());
        }

        /// <summary>
        /// Called to confirm that the IPC channel is still open / host application has not closed
        /// </summary>
        public void Ping()
        {
            // Output token animation to visualise Ping
            var oldTop = Console.CursorTop;
            var oldLeft = Console.CursorLeft;
            Console.CursorVisible = false;

            var chars = "\\|/-";
            Console.Write(chars[_count++ % chars.Length]);

            Console.SetCursorPosition(oldLeft, oldTop);
            Console.CursorVisible = true;
        }
    }
}
