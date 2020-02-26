using System.Collections.Generic;
using System.Text.Json;

namespace SocketHook.Extensions.Options
{
    public class InjectOptions
    {
        public int RedirectionPort { get; }
        public IEnumerable<string> RedirectedIps { get; }

        public InjectOptions(int redirectionPort, IEnumerable<string> redirectedIps)
        {
            RedirectionPort = redirectionPort;
            RedirectedIps = redirectedIps;
        }

        public override string ToString() => _serializedJson ??= JsonSerializer.Serialize(this);
        private string _serializedJson;
    }
}
