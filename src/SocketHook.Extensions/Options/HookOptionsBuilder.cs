using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SocketHook.Extensions.Services;

namespace SocketHook.Extensions.Options
{
    public sealed class HookOptionsBuilder
    {
        private HookOptions _settings;

        internal HookOptionsBuilder()
        {
        }

        /// <summary>
        /// Bind the configuration to the a new <see cref="HookOptions"/>, if <see cref="fromSection"/>
        /// is set to true, the options will be bound from a section of the configuration.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="fromSection">Determine whether it should be bound from a section or not.</param>
        /// <returns></returns>
        public HookOptionsBuilder AddConfiguration(IConfiguration configuration, bool fromSection = false)
        {
            if (fromSection) configuration = configuration.GetSection(nameof(HookOptions));
            var options = new HookOptions();
            configuration.Bind(options);
            _settings = options;
            return this;
        }

        public HookOptionsBuilder Configure(Action<HookOptions> configure)
        {
            if (configure == null) return this;
            var options = _settings ??= new HookOptions();
            configure(options);
            return this;
        }
        internal IServiceCollection RegisterAllDependencies(IServiceCollection services)
        {
            if (string.IsNullOrWhiteSpace(_settings.ExePath)) throw new ArgumentNullException(nameof(_settings.ExePath));
            if (!File.Exists(_settings.ExePath)) throw new FileNotFoundException($"Couldn't find hook executable at {_settings.ExePath}");
            services.TryAddSingleton<IHookLifetimeService>(sp => new HookLifetimeService(_settings.ExePath));

            if (_settings.UseHookServiceFactory)
                services.AddHttpClient<ISocketHookServiceFactory, SocketHookServiceFactory>(ConfigureHttpClient)
                    .SetHandlerLifetime(Timeout.InfiniteTimeSpan);

            if (_settings.HasValidInjectOptions())
                services.AddHttpClient<ISocketHookService, SocketHookService>(ConfigureHttpClient)
                    .SetHandlerLifetime(Timeout.InfiniteTimeSpan);
            
            var isStartupStringValid = !string.IsNullOrWhiteSpace(_settings.InjectToExeOnStartup) && _settings.InjectToExeOnStartup.EndsWith(".exe");
            var isStartupExeValid = isStartupStringValid && File.Exists(_settings.InjectToExeOnStartup);
            if (!isStartupExeValid) _settings.InjectToExeOnStartup = string.Empty;

            services.AddHostedService(sp =>
            {
                ISocketHookService hookService;
                if (isStartupExeValid && _settings.HasValidInjectOptions()) 
                    hookService = sp.GetRequiredService<ISocketHookService>();
                else
                    hookService = sp.GetRequiredService<ISocketHookServiceFactory>()
                        .CreateKillAllOnlyService();

                return new SocketHookHosting(_settings, 
                    sp.GetRequiredService<IHookLifetimeService>(), hookService, 
                    sp.GetRequiredService<IHostEnvironment>());
            });

            return services;
        }

        private void ConfigureHttpClient(HttpClient client)
        {
            var port = 80;
            if (_settings.ApiPort != default) port = _settings.ApiPort;
            client.BaseAddress = new Uri($"http://127.0.0.1:{port}/api/");
        }
    }
}
