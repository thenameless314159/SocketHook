using EasyHook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using static SocketHook.NativeSocketMethods;
using static EasyHook.RemoteHooking;

namespace SocketHook
{
    public class Main : IEntryPoint
    {
        private IEnumerable<IPAddress> _whitelist;
        private HookInterface _interface;
        private LocalHook _connectHook;
        private ushort _redirectionPort;

        public Main(IContext context, string channelName, IEnumerable<string> ipsWhitelist, int redirectionPort)
        {
            _interface = IpcConnectClient<HookInterface>(channelName);
            _whitelist = ipsWhitelist.Select(IPAddress.Parse);
            _redirectionPort = (ushort)redirectionPort;

            _interface.Ping();
        }

        public void Run(IContext context, string channelName, IEnumerable<string> ipsWhitelist, int redirectionPort)
        {
            var currentProcess = Process.GetCurrentProcess();
            _interface.NotifyInstalled(currentProcess.ProcessName, currentProcess.Id);
            
            try
            {
                _connectHook = LocalHook.Create(
                    LocalHook.GetProcAddress("Ws2_32.dll", "connect"),
                    new WinsockConnectDelegate(_onConnect), this);

                _connectHook.ThreadACL.SetExclusiveACL(new[] { 0 });
            }
            catch (Exception ex) { _interface.OnError(ex); }

            WakeUpProcess();
            while (true) Thread.Sleep(1000);
        }

        private int _onConnect(IntPtr socket, IntPtr address, int addrSize)
        {
            var structure = Marshal.PtrToStructure<sockaddr_in>(address);
            var ipAddress = new IPAddress(structure.sin_addr.S_addr);
            var port = structure.sin_port;

            if (!_whitelist.Contains(ipAddress)) return connect(socket, address, addrSize);

            _interface.Message($"Connection attempt at {ipAddress}:{port}, redirecting to 127.0.0.1:{_redirectionPort}...");
            var strucPtr = Marshal.AllocHGlobal(addrSize);
            var struc = new sockaddr_in
            {
                sin_addr = { S_addr = inet_addr("127.0.0.1") },
                sin_port = htons(_redirectionPort),
                sin_family = (short)AddressFamily.InterNetworkv4,
            };

            Marshal.StructureToPtr(struc, strucPtr, true);
            return connect(socket, strucPtr, addrSize);
        }
    }
}
