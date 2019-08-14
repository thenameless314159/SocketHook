using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketHook
{
    public class HookSettings
    {
        public string ExePath { get; set; }
        public int RedirectedPort { get; set; }
        public IEnumerable<string> RedirectedIps { get; set; }
    }
}
