using Microsoft.Extensions.DependencyInjection;
using iControl.Desktop.Core;
using iControl.Desktop.Core.Interfaces;
using iControl.Desktop.Core.Services;
using iControl.Shared.Utilities;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace iControl.Desktop.XPlat.Services
{
    public class ShutdownServiceLinux : IShutdownService
    {
        public async Task Shutdown()
        {
            Logger.Debug($"Exiting process ID {Environment.ProcessId}.");
            var casterSocket = ServiceContainer.Instance.GetRequiredService<ICasterSocket>();
            await casterSocket.DisconnectAllViewers();
            Environment.Exit(0);
        }
    }
}
