#if LOCAL_DEV
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace PitBoss
{
    public class DefaultContainerBalancer: IContainerBalancer
    {
        private IContainerManager _containerManager;
        private IPipelineManager _pipelineManager;
        private ILogger _logger;
        private IHttpClientFactory _factory;
        private IConfiguration _configuration;

        public DefaultContainerBalancer(
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
        }

        public void BalanceGroup(IOperationGroup group)
        {
            BalanceGroupAsync(group).Wait();
        }

        public async Task BalanceGroupAsync(IOperationGroup group)
        {
            var currentCount = await group.CurrentSizeAsync();
            if(currentCount < group.TargetSize && !group.Stale)
            {
                for(var x = currentCount; x < group.TargetSize; x++)
                {
                    await group.AddContainerAsync(new DefaultOperationContainer(group.PipelineStep, _factory, _logger, _configuration));
                }
            }
            if(group.Stale)
            {
                var containers = await group.GetContainersAsync();
                foreach(var container in containers)
                {
                    await group.RemoveContainerAsync(container);
                }
            }
        }
    }
}
#endif