using System;
using System.Web.Http;
using System.Web.Http.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SocketHook.API.Services;

namespace SocketHook.API.Controllers
{
    public class KillAllController : ApiController
    {
        protected static IHookService HookService { get; private set; }
        protected static ILogger Logger { get; private set; }

        public static void SetupDependencies(IServiceProvider services)
        {
            Logger = services.GetRequiredService<ILogger<KillAllController>>();
            HookService = services.GetRequiredService<IHookService>();
        }

        public IHttpActionResult Delete()
        {
            Logger.LogInformation("Kill all registered processes directive received !");
            try
            {

                HookService.KillAllInjectedProcesses();
                return Ok();
            }
            catch (Exception e)
            {
                return new ExceptionResult(e, this);
            }
        }
    }
}
