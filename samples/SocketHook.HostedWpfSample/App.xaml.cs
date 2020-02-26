using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SocketHook.Extensions;
using SocketHook.HostedWpfSample.Services;
using SocketHook.HostedWpfSample.ViewModels;
using SocketHook.HostedWpfSample.Views;

namespace SocketHook.HostedWpfSample
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IHost RunningHost { get; private set; } // keeps a lifetime instance

        private static IHost CreateHost(string[] args) =>
           Host.CreateDefaultBuilder(args)
               //.ConfigureHostConfiguration(config => config.AddCommandLine(args))
               .ConfigureAppConfiguration(config =>
               {
                   config.SetBasePath(Directory.GetCurrentDirectory());
                   config.AddJsonFile("appsettings.json", optional: true);
                   config.AddCommandLine(args);
               })
               .ConfigureServices((ctx, services) =>
               {
                   if(ctx.HostingEnvironment.IsDevelopment())
                       services.AddLogging(logger =>
                       {
                           logger.SetMinimumLevel(LogLevel.Trace);
                           logger.AddFile($"app{DateTime.Now:yyyy-dd-M--HH-mm-ss}.log");
                       });
                   
                   services.AddSocketHook(opt =>
                   {
                       opt.AddConfiguration(ctx.Configuration);
                       opt.Configure(x =>
                       {
                           x.UseHookServiceFactory = true;
                           x.OpenHookOnStartup = true;
                           x.KillAllOnExit = true;
                       });
                   });

                   services.AddSingleton<IProcessObserverService, ProcessObserverService>();
                   services.AddSingleton<IInjectOptionsService, InjectOptionsService>();
                   services.AddTransient<InjectOptionsViewModel>();
                   services.AddTransient<MainWindowViewModel>();
                   services.AddTransient<InjectOptionsView>();
                   services.AddTransient<MainWindow>();
               }).Build();

        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += (o, s) =>
            {
                if (s.Handled) return;
                MessageBox.Show($"An unhandled exception was propagated to the UI thread :\n{s.Exception}", "Fatal error",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                RunningHost?.StopAsync()?.GetAwaiter().GetResult();
                RunningHost?.Dispose();
            };

            RunningHost = CreateHost(e.Args);
            RunningHost.Start();

            var mainWindow = RunningHost.Services.GetRequiredService<MainWindow>();
            MainWindow = mainWindow;
            mainWindow.Show();
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try { RunningHost.Dispose(); } catch { /* discarded */ }
            base.OnExit(e);
        }
    }
}
