using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SocketHook.API.Models;
using SocketHook.API.Services;

namespace SocketHook.API.Controllers
{
    public class InjectController : ApiController
    {
        protected static IHookService HookService { get; private set; }

        public static void SetupDependencies(IServiceProvider services) =>
            HookService = services.GetRequiredService<IHookService>();

        public IHttpActionResult Put(int pId, [FromBody]InjectionSettings options)
        {
            try
            {
                if (!HookService.TryInject(pId, options.RedirectionPort, options.RedirectedIps.ToArray()))
                    return BadRequest("Process has already been injected");

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
