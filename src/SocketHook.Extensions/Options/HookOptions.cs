using System.Collections.Generic;
using System.Linq;

namespace SocketHook.Extensions.Options
{
    public sealed class HookOptions
    {
        public int ApiPort { get; set; }
        public string ExePath { get; set; }

        public bool KillAllOnExit { get; set; } = true;
        public bool OpenHookOnStartup { get; set; }
        public bool UseHookServiceFactory { get; set; }

        /// <summary>
        /// Requires <see cref="RedirectionPort"/> and <see cref="RedirectedIps"/>
        /// to be set and valid.
        /// </summary>
        public string InjectToExeOnStartup { get; set; }

        public int RedirectionPort { get; set; }
        public IEnumerable<string> RedirectedIps { get; set; }
    }

    #region Extensions

    public static class HookOptionsExtensions
    {
        public static bool HasValidInjectOptions(this HookOptions opt) => opt.RedirectionPort != default &&
                                                                          opt.RedirectedIps != null &&
                                                                          opt.RedirectedIps.Any();

        public static InjectOptions AsInjectOptions(this HookOptions opt) => 
            new InjectOptions(opt.RedirectionPort, opt.RedirectedIps);
    }

    #endregion
}
