using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketHook
{
    public class HookInterface : MarshalByRefObject
    {
        private int _count;

        public void NotifyInstalled(string processName) => Console.WriteLine($"Successfully injected to {processName}.exe !");

        public void Message(string message) => Console.WriteLine(message);

        public void OnError(Exception ex) => Console.WriteLine(ex.ToString());

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
