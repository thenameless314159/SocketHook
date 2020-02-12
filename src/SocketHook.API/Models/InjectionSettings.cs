using System.Collections.Generic;

namespace SocketHook.API.Models
{
    public class InjectionSettings
    {
        public int RedirectionPort { get; set; }
        public IEnumerable<string> RedirectedIps { get; set; }
    }
}
