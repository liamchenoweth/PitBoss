using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PitBoss
{
    public class ContainerServiceContainer
    {
        public async static Task Main(string[] args)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            switch(Environment.GetEnvironmentVariable("PITBOSS_IMPLEMENTATION"))
            {
                case "DOCKER_COMPOSE":
                    await StartServiceServerDockerCompose(80, source.Token);
                    break;
                case "KUBERNETES":
                    await StartServiceServerKubernetes(80, source.Token);
                    break;
            }
        }

        public static async Task StartServiceServerDockerCompose(int port, CancellationToken token)
        {
            BossWebServer server = new BossWebServer();
            server.UseContainerBalancer<DockerComposeContainerBalancer>();
            server.UseContainerManager<DockerComposeContainerManager>();
            server.ConfigureServices(x => x.AddSingleton<IContainerService, ContainerService>());
            var host = server.StartWebHost("0.0.0.0", port);
            var logger = host.Services.GetService<ILogger<ContainerServiceContainer>>();
            logger.LogInformation($"Starting new Container Service server on url: http://localhost:{port}");

            await host.Services.GetService<IContainerService>().BalanceContainers(token);
        }

        public static async Task StartServiceServerKubernetes(int port, CancellationToken token)
        {
            BossWebServer server = new BossWebServer();
            server.UseContainerBalancer<KubernetesContainerBalancer>();
            server.UseContainerManager<KubernetesContainerManager>();
            server.ConfigureServices(x => x.AddSingleton<IContainerService, ContainerService>());
            var host = server.StartWebHost("0.0.0.0", port);
            var logger = host.Services.GetService<ILogger<ContainerServiceContainer>>();
            logger.LogInformation($"Starting new Container Service server on url: http://localhost:{port}");

            await host.Services.GetService<IContainerService>().BalanceContainers(token);
        }

        #if LOCAL_DEV
        public static async Task StartServiceServer(int port, IDistributedService memoryService, CancellationToken token)
        {
            BossWebServer server = new BossWebServer();
            server.ConfigureDistributedService(memoryService);
            server.UseContainerBalancer<DefaultContainerBalancer>();
            server.UseContainerManager<DefaultContainerManager>();
            server.ConfigureServices(x => x.AddSingleton<IContainerService, ContainerService>());
            var host = server.StartWebHost("0.0.0.0", port);
            var logger = host.Services.GetService<ILogger<ContainerServiceContainer>>();
            logger.LogInformation($"Starting new Container Service server on url: http://localhost:{port}");

            await host.Services.GetService<IContainerService>().BalanceContainers(token);
        }
        #endif
    }
}
