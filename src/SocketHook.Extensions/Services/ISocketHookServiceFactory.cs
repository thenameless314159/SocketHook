using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using SocketHook.Extensions.Options;

namespace SocketHook.Extensions.Services
{
    public interface ISocketHookServiceFactory
    {
        ISocketHookService CreateWith(InjectOptions options);
    }

    internal sealed class SocketHookServiceFactory : ISocketHookServiceFactory
    {
        private readonly IHookLifetimeService _hookLifetime;
        private readonly HttpClient _client;

        public SocketHookServiceFactory(HttpClient client, IHookLifetimeService hookLifetime)
        {
            _hookLifetime = hookLifetime;
            _client = client;
        }

        public ISocketHookService CreateWith(InjectOptions options)
        {
            if (options.RedirectedIps == null || !options.RedirectedIps.Any())
                throw new ArgumentNullException(nameof(options.RedirectedIps));
            if (options.RedirectionPort == default) throw new ArgumentNullException(nameof(options.RedirectionPort));
            return new SocketHookService(_client, _hookLifetime, options);
        }

        internal ISocketHookService CreateKillAllOnlyService() => new SocketHookService(_client, _hookLifetime);
    }

    #region Extensions

    public static class SocketHookServiceFactoryExtensions
    {
        public static ISocketHookService CreateWith(this ISocketHookServiceFactory factory, IEnumerable<string> ips,
            int redirectedToPort) => factory.CreateWith(new InjectOptions(redirectedToPort, ips));

        public static ISocketHookService CreateWith(this ISocketHookServiceFactory factory, IEnumerable<IPAddress> ips,
            int redirectedToPort) => factory.CreateWith(new InjectOptions(redirectedToPort, ips.Select(ip => ip.ToString())));

        internal static ISocketHookService CreateKillAllOnlyService(this ISocketHookServiceFactory factory) =>
            ((SocketHookServiceFactory) factory).CreateKillAllOnlyService();
    }

    #endregion
}
