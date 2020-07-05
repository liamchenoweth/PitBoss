using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        public static CancellationTokenSource serviceToken;
        public static async Task Main()
        {
            CancellationTokenSource source = new CancellationTokenSource();
            serviceToken = source;
            await StartJobServer(80, source.Token);
        }

        public static async Task StartJobServer(int port, CancellationToken token)
        {
            OperationWebServer server = new OperationWebServer();
            var host = server.StartWebHost(port);
            var logger = host.Services.GetService<ILogger<Job>>();
            logger.LogInformation($"Starting new job server on url: http://localhost:{port}");
            
            await host.Services.GetService<IOperationService>().PollRequests(token);
        }
    }
}
