using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PitBoss;

namespace JobContainer
{
    public class Job
    {
        public static void Main()
        {
            var host = StartJobServer(0);
            host.WaitForShutdown();
        }

        public static IWebHost StartJobServer(int port)
        {
            OperationWebServer server = new OperationWebServer();
            var host = server.StartWebHost(port);
            var logger = host.Services.GetService<ILogger<Job>>();
            logger.LogInformation($"Starting new job server on port: {host.ServerFeatures.Get<IServerAddressesFeature>().Addresses.FirstOrDefault()}");
            return host;
        }
    }
}
