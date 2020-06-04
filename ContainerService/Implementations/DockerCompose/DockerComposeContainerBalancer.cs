using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Docker.DotNet;

namespace PitBoss
{
    public class DockerComposeContainerBalancer: IContainerBalancer
    {
        private IContainerManager _containerManager;
        private IPipelineManager _pipelineManager;
        private ILogger _logger;
        private IHttpClientFactory _factory;
        private IConfiguration _configuration;
        private DockerClient _docker;

        public DockerComposeContainerBalancer(
            IContainerManager containerManager,
            IPipelineManager pipelineManager,
            IHttpClientFactory factory,
            ILogger<IContainerBalancer> logger,
            IConfiguration configuration
        )
        {
            _containerManager = containerManager;
            _pipelineManager = pipelineManager;
            _logger = logger;
            _factory = factory;
            _configuration = configuration;
            _docker = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();
        }

        public void BalanceGroup(IOperationGroup group)
        {
            BalanceGroupAsync(group).Wait();
        }

        public async Task BalanceGroupAsync(IOperationGroup group)
        {
            var currentCount = await group.CurrentSizeAsync();
            if(currentCount < group.TargetSize)
            {
                for(var x = currentCount; x < group.TargetSize; x++)
                {
                    await group.AddContainerAsync(new DockerComposeOperationContainer(group.PipelineStep, _factory, _configuration, _logger, _docker));
                }
            }
        }
    }
}