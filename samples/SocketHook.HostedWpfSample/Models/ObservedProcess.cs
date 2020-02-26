using System.Drawing;

namespace SocketHook.HostedWpfSample.Models
{
    public class ObservedProcess
    {
        public string Name { get; set; }
        public Bitmap Icon { get; set; }
        public uint Priority { get; set; }
        public uint ProcessId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }
}
