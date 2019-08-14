using System;
using System.Runtime.InteropServices;

namespace SocketHook
{
    public static class NativeSocketMethods
    {
        [DllImport("Ws2_32.dll", CharSet = CharSet.Ansi)]
        public static extern uint inet_addr(string cp);

        [DllImport("Ws2_32.dll")]
        public static extern ushort htons(ushort hostshort);

        public enum AddressFamily
        {
            AppleTalk = 0x11,
            BlueTooth = 0x20,
            InterNetworkv4 = 2,
            InterNetworkv6 = 0x17,
            Ipx = 4,
            Irda = 0x1a,
            NetBios = 0x11,
            Unknown = 0
        }

        [DllImport("WS2_32.dll", SetLastError = true)]
        public static extern int connect(IntPtr s, IntPtr addr, int addrsize);

        [DllImport("Ws2_32.dll")]
        public static extern int send(IntPtr s, IntPtr buf, int len, int flags);

        [DllImport("Ws2_32.dll")]
        public static extern int recv(IntPtr s, IntPtr buf, int len, int flags);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi, SetLastError = true)]
        public delegate int WinsockConnectDelegate(IntPtr s, IntPtr addr, int addrsize);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi, SetLastError = true)]
        public delegate int WinsockRecvDelegate(IntPtr s, IntPtr buf, int len, int flags);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi, SetLastError = true)]
        public delegate int WinsockSendDelegate(IntPtr s, IntPtr buf, int len, int flags);

        public enum ProtocolType
        {
            BlueTooth = 3,
            ReliableMulticast = 0x71,
            Tcp = 6,
            Udp = 0x11
        }

        [StructLayout(LayoutKind.Sequential, Size = 16)]
        public struct sockaddr_in
        {
            public const int Size = 16;

            public short sin_family;
            public ushort sin_port;
            public struct in_addr
            {
                public uint S_addr;
                public struct _S_un_b
                {
                    public byte s_b1, s_b2, s_b3, s_b4;
                }
                public _S_un_b S_un_b;
                public struct _S_un_w
                {
                    public ushort s_w1, s_w2;
                }
                public _S_un_w S_un_w;
            }
            public in_addr sin_addr;
        }

        public enum SocketType
        {
            Unknown,
            Stream,
            DGram,
            Raw,
            Rdm,
            SeqPacket
        }
    }
}
