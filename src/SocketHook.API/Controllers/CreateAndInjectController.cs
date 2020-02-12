using System;
using System.Web.Http;
using System.Web.Http.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SocketHook.API.Models;
using SocketHook.API.Services;

namespace SocketHook.API.Controllers
{
    public class CreateAndInjectController : ApiController
    {
        protected static IHookService HookService { get; private set; }

        public static void SetupDependencies(IServiceProvider services) =>
            HookService = services.GetRequiredService<IHookService>();

        public IHttpActionResult Get(string exePath, [FromBody]InjectionSettings options)
        {
            try
            {
                if (!HookService.TryCreateAndInject(exePath, options.RedirectionPort, options.RedirectedIps.ToString()))
                    return BadRequest($"Couldn't find exe at {exePath} !");

                return Ok();
            }
            catch (ArgumentException e)
            {
                return BadRequest($"Invalid {e.ParamName} provided !");
            }
            catch (Exception e)
            {
                return new ExceptionResult(e, this);
            }
        }
    }
}
