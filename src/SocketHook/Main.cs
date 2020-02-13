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
        private readonly HookInterface _logger;
        private LocalHook _connectHook;
        private ushort _redirectionPort;

        public Main(IContext context, string channelName, IEnumerable<string> ipsWhitelist, int redirectionPort)
        {
            _logger = IpcConnectClient<HookInterface>(channelName);
        }

        public void Run(IContext context, string channelName, IEnumerable<string> ipsWhitelist, int redirectionPort)
        {
            try
            {
                _whitelist = ipsWhitelist.ToList().Select(IPAddress.Parse);
                _redirectionPort = (ushort)redirectionPort;

                _connectHook = LocalHook.Create(
                    LocalHook.GetProcAddress("Ws2_32.dll", "connect"),
                    new WinsockConnectDelegate(_onConnect), this);

                WakeUpProcess(); 
                var currentProcess = Process.GetCurrentProcess(); 
                _logger.NotifyInstalled(currentProcess.ProcessName, currentProcess.Id);
                _connectHook.ThreadACL.SetExclusiveACL(new[] { 0 });
            }
            catch (Exception ex) { _logger.OnError(ex); }
            while (true) Thread.Sleep(1000);
        }

        private int _onConnect(IntPtr socket, IntPtr address, int addrSize)
        {
            try
            {
                var structure = Marshal.PtrToStructure<sockaddr_in>(address);
                var ipAddress = new IPAddress(structure.sin_addr.S_addr);
                var port = structure.sin_port;
                _logger.LogInformation($"Connection attempt at {ipAddress}:{port} successfully intercepted !");

                if (!_whitelist.Contains(ipAddress)) 
                {
                    _logger.LogWarning("Address wasn't in registered whitelist, the client will connect to it directly.");
                    return connect(socket, address, addrSize);
                }

                var strucPtr = Marshal.AllocHGlobal(addrSize);
                var struc = new sockaddr_in
                {
                    sin_addr = {S_addr = inet_addr("127.0.0.1")},
                    sin_port = htons(_redirectionPort),
                    sin_family = (short) AddressFamily.InterNetworkv4,
                };
                _logger.LogDebug($"Intercepted address successfully rewritten to 127.0.0.1:{_redirectionPort}, attempting to connect...");
                Marshal.StructureToPtr(struc, strucPtr, true);
                return connect(socket, strucPtr, addrSize);
            }
            catch (Exception e)
            {
                _logger.OnError(e);
                return default;
            }
        }
    }
}
