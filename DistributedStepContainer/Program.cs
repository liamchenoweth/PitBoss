using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PitBoss
{
    public class DistributedStepContainer
    {
        public async static Task Main(string[] args)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            // TODO: Add Configuration
            await StartServiceServer(80, source.Token);
        }

        public static async Task StartServiceServer(int port, CancellationToken token)
        {
            BossWebServer server = new BossWebServer();
            server.ConfigureServices(x => x.AddSingleton<IDistributedStepService, DistributedStepService>());
            var host = server.StartWebHost("0.0.0.0", port);
            var logger = host.Services.GetService<ILogger<DistributedStepContainer>>();
            logger.LogInformation($"Starting new Ditributed Step Container server on url: http://localhost:{port}");

            await host.Services.GetService<IDistributedStepService>().MonitorDistributedRequests(token);
        }

        #if LOCAL_DEV
        public static async Task StartServiceServer(int port, IDistributedService memoryService, CancellationToken token)
        {
            BossWebServer server = new BossWebServer();
            server.ConfigureDistributedService(memoryService);
            server.ConfigureServices(x => x.AddSingleton<IDistributedStepService, DistributedStepService>());
            var host = server.StartWebHost("0.0.0.0", port);
            var logger = host.Services.GetService<ILogger<DistributedStepContainer>>();
            logger.LogInformation($"Starting new Ditributed Step Container server on url: http://localhost:{port}");

            await host.Services.GetService<IDistributedStepService>().MonitorDistributedRequests(token);
        }
        #endif

    }
}
