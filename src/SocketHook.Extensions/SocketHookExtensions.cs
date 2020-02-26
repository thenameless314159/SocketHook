using System;
using Microsoft.Extensions.DependencyInjection;
using SocketHook.Extensions.Options;

namespace SocketHook.Extensions
{
    public static class SocketHookExtensions
    {
        public static IServiceCollection AddSocketHook(this IServiceCollection services, Action<HookOptionsBuilder> configure)
        {
            if (configure == null) return services;
            var options = new HookOptionsBuilder();
            configure(options);

            return options.RegisterAllDependencies(services);
        }
    }
}
