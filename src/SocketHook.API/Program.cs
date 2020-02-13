using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SocketHook.API.Services;

namespace SocketHook.API
{
    internal class Program
    {
        private static Task Main(string[] args)
        {
            SetupConsole();
            return CreateHostBuilder(args).RunConsoleAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(config => config.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true)
                    .AddCommandLine(args))
                .ConfigureLogging((ctx, logger) => 
                {
                    if (ctx.HostingEnvironment.IsDevelopment()) logger.SetMinimumLevel(LogLevel.Debug);
                    else logger.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning);
                    logger.AddFilter("Microsoft.Extensions.Hosting.Internal.Host", LogLevel.Warning);
                    logger.AddConsole();
                })
                .ConfigureServices((ctx, services) =>
                {
                    var settings = new Settings();
                    ctx.Configuration.Bind(settings);
                    var socketHookPath = Directory.GetCurrentDirectory() + @"\SocketHook.dll";
                    if (File.Exists(socketHookPath)) settings.HookPath = socketHookPath;

                    services.AddSingleton(settings);
                    services.AddTransient(sp =>
                    {
                        var cfg = sp.GetRequiredService<Settings>();
                        var port = cfg.Port != default
                            ? cfg.Port
                            : 80;

                        return new HttpServerLifeTimeService(port,
                            sp.GetRequiredService<ILogger<HttpServerLifeTimeService>>());
                    });

                    services.AddHostedService<ControllersDependencyInjectionService>();
                    services.AddHostedService<ApplicationStartupService>();
                    services.AddSingleton<IHookService, HookService>();
                });

        static void SetupConsole()
        {
            Console.Title = "Socket hook API";
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(@"
  |________|___________________|_
  |        | | | | | | | | | | | |________________
  |________|___________________|_|                ,
  |        |                   |                   ");
            Console.WriteLine("  Socket hook injector - github.com/thenameless314159\n");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
