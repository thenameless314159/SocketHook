using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;
using Microsoft.Extensions.Logging;

namespace SocketHook.API.Services
{
    internal sealed class HttpServerLifeTimeService : IAsyncDisposable
    {
        private readonly string _baseAddress;
        private HttpSelfHostServer _server;
        private readonly ILogger _logger;

        public HttpServerLifeTimeService(int listeningPort, ILogger logger)
        {
            _baseAddress = $"http://127.0.0.1:{listeningPort}/";
            _logger = logger;
        }

        public async Task SetupAsync()
        {
            var config = new HttpSelfHostConfiguration(_baseAddress);
            config.Routes.MapHttpRoute("API Default", 
                "api/{controller}/{id}",
                new { id = RouteParameter.Optional });

            _server = new HttpSelfHostServer(config);
            await _server.OpenAsync();

            _logger.LogInformation($"Now listening at : {_baseAddress} !");
        }

        public ValueTask DisposeAsync()
        {
            return _server == default ? default : closeServer();
            async ValueTask closeServer() { try { await _server.CloseAsync(); _server.Dispose(); } catch{ /*discarded */ } }
        }
    }
}
