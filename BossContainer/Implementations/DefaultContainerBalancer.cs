using System.Net.Http;
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

        public DefaultContainerBalancer(
            IContainerManager containerManager,
            IPipelineManager pipelineManager,
            IHttpClientFactory factory,
            ILogger logger
        )
        {
            _containerManager = containerManager;
            _pipelineManager = pipelineManager;
            _logger = logger;
            _factory = factory;
        }

        public void BalanceGroup(IOperationGroup group)
        {

        }

        public async Task BalanceGroupAsync(IOperationGroup group)
        {
            var currentCount = await group.CurrentSizeAsync();
            if(currentCount < group.TargetSize)
            {
                for(var x = currentCount; x < group.TargetSize; x++)
                {
                    group.AddContainer(new DefaultOperationContainer(group.PipelineStep, _factory));
                }
            }
        }
    }
}