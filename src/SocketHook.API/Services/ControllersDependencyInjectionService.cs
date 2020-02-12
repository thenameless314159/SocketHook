using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using SocketHook.API.Controllers;

namespace SocketHook.API.Services
{
    internal sealed class ControllersDependencyInjectionService : IHostedService
    {
        public ControllersDependencyInjectionService(IServiceProvider provider)
        {
            CreateAndInjectController.SetupDependencies(provider);
            KillAllController.SetupDependencies(provider);
            InjectController.SetupDependencies(provider);
        }

        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
