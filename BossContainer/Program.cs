using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PitBoss.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace PitBoss
{
    public class Boss
    {
        public async static Task Main(string[] args)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            var configPath = Environment.GetEnvironmentVariable("PITBOSS_CONFIGURATION");
            var config = new ConfigurationBuilder()
                .AddJsonFile(Path.IsPathRooted(configPath) ? configPath : $"{FileUtils.GetBasePath()}/{configPath}", false, true)
                .Build();
            await StartServiceServer(config.GetValue<int>("OperationGroupContainer:Port"), source.Token);
        }

        public static async Task StartServiceServer(int port, CancellationToken token)
        {
            BossWebServer server = new BossWebServer();
            var host = server.StartWebHost("0.0.0.0", port);
            var logger = host.Services.GetService<ILogger<Boss>>();
            logger.LogInformation($"Starting new Container Service server on url: http://localhost:{port}");
            // Compile pipelines
            var pipelineLocation = host.Services.GetService<IConfiguration>()["Boss:Pipelines:Location"];
            if(!Path.IsPathRooted(pipelineLocation))
            {
                pipelineLocation = $"{FileUtils.GetBasePath()}/{pipelineLocation}";
            }
            await host.Services.GetService<IPipelineManager>().CompilePipelinesAsync(pipelineLocation);
            await host.WaitForShutdownAsync();
        }

        #if LOCAL_DEV
        public static async Task StartServiceServer(int port, IDistributedService memoryService, CancellationToken token)
        {
            BossWebServer server = new BossWebServer();
            server.ConfigureDistributedService(memoryService);
            var host = server.StartWebHost("0.0.0.0", port);
            var logger = host.Services.GetService<ILogger<Boss>>();
            logger.LogInformation($"Starting new Container Service server on url: http://localhost:{port}");
            // Compile pipelines
            var pipelineLocation = host.Services.GetService<IConfiguration>()["Boss:Pipelines:Location"];
            if(!Path.IsPathRooted(pipelineLocation))
            {
                pipelineLocation = $"{FileUtils.GetBasePath()}/{pipelineLocation}";
            }
            await host.Services.GetService<IPipelineManager>().CompilePipelinesAsync(pipelineLocation);
            await host.WaitForShutdownAsync();
        }
        #endif
    }
}
