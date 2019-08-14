using EasyHook;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketHook.Injector
{
    class Program
    {
        const string hookPath = @".\SocketHook.dll";

        static IpcServerChannel _ipcServer; // keep alive
        static string _channelName;

        static void Main(string[] args)
        {
            SetupConsole();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("hooksettings.json", optional: false)
                .Build();

            var settings = new HookSettings();
            configuration.Bind(settings);

            try
            {
                _ipcServer = RemoteHooking.IpcCreateServer<HookInterface>(ref _channelName, WellKnownObjectMode.Singleton);
                RemoteHooking.CreateAndInject(
                    settings.ExePath, 
                    string.Empty, 
                    0x00000004, 
                    InjectionOptions.DoNotRequireStrongName, 
                    hookPath, 
                    hookPath, 
                    out var pId, 
                    _channelName, 
                    settings.RedirectedIps, 
                    settings.RedirectionPort);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            
            while (true) Thread.Sleep(1000); // TODO: handle close request etc.
        }

        static void SetupConsole()
        {
            Console.Title = "Socket hook injector";
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(@"
  |________|___________________|_
  |        | | | | | | | | | | | |________________
  |________|___________________|_|                ,
  |        |                   |                   ");
            Console.WriteLine($"  SocketHook injector by thenameless135159\n");
        }
    }
}
