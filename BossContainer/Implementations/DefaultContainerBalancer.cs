using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace PitBoss
{
    public class DefaultContainerBalancer: IContainerBalancer
    {
        private IContainerManager _containerManager;
        private IPipelineManager _pipelineManager;
        private ILogger _logger;

        public DefaultContainerBalancer(
            IContainerManager containerManager,
            IPipelineManager pipelineManager,
            ILogger logger
        )
        {
            _containerManager = containerManager;
            _pipelineManager = pipelineManager;
            _logger = logger;
        }

        public void BalanceGroup(IOperationGroup group)
        {

        }

        public async Task BalanceGroupAsync(IOperationGroup group)
        {
            if(!_pipelineManager.Ready)
            {
                _logger.LogWarning("Pipelines not yet compiled, cannot balance containers");
            }
            
        }
    }
}