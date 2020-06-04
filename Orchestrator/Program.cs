using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PitBoss;
using PitBoss.Utils;


namespace Orchestrator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configPath = Environment.GetEnvironmentVariable("PITBOSS_CONFIGURATION");
            var config = new ConfigurationBuilder()
                .AddJsonFile(Path.IsPathRooted(configPath) ? configPath : $"{FileUtils.GetBasePath()}/{configPath}", false, true)
                .Build();

            var tokenSource = new CancellationTokenSource();
            IDistributedService memoryService = new MemoryDistributedService();
            #if LOCAL_DEV
            var groupContainer = OperationGroupContainer.StartServiceServer(config.GetValue<int>("OperationGroupContainer:Port"), memoryService, tokenSource.Token);
            var stepContainer = DistributedStepContainer.StartServiceServer(config.GetValue<int>("DistributedStepContainer:Port"), memoryService, tokenSource.Token);
            var containerService = ContainerServiceContainer.StartServiceServer(config.GetValue<int>("ContainerService:Port"), memoryService, tokenSource.Token);
            var boss = Boss.StartServiceServer(config.GetValue<int>("Boss:Host:Port"), memoryService, tokenSource.Token);
            await Task.WhenAll(groupContainer, stepContainer, containerService, boss);
            tokenSource.Cancel();
            #endif
        }
    }
}
