using System.Collections.Generic;
using System.Linq;

namespace SocketHook.API
{
    public sealed class Settings
    {
        public int Port { get; set; }
        public string HookPath { get; set; }
        public int InjectToPId { get; set; }
        public string InjectToExe { get; set; }
        public int RedirectionPort { get; set; }
        public IEnumerable<string> RedirectedIps { get; set; }
    }

    #region Extensions

    public static class SettingsExtensions
    {
        public static bool ShouldInjectToPId(this Settings s) => s.InjectToPId != default;
        public static bool ShouldInjectToExe(this Settings s) => !string.IsNullOrWhiteSpace(s.InjectToExe);
        public static bool AreInjectInputsValid(this Settings s) => s.RedirectionPort == default || 
                                                                    s.RedirectedIps == null ||
                                                                    !s.RedirectedIps.Any();
    }

    #endregion
}
