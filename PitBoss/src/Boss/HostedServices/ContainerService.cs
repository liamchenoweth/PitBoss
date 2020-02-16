using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using Serilog;

namespace PitBoss
{
    public class ContainerService : BackgroundService
    {
        private IContainerManager _containerManager;
        private IContainerBalancer _containerBalancer;
        private IPipelineManager _pipelineManager;
        private ILogger<ContainerService> _logger;

        public ContainerService(
            ILogger<ContainerService> logger,
            IContainerManager containerManager,
            IContainerBalancer containerBalancer,
            IPipelineManager pipelineManager)
        {
            _logger = logger;
            _containerManager = containerManager;
            _containerBalancer = containerBalancer;
            _pipelineManager = pipelineManager;
        }

        protected async override Task ExecuteAsync(CancellationToken cancelationToken)
        {
            while(!_pipelineManager.Ready)
            {
                _logger.LogInformation("Waiting for pipelines to be compiled");
                await Task.Delay(5000); // TODO: Maybe make this configurable
            }

            foreach(var pipeline in _pipelineManager.Pipelines)
            {
                foreach(var step in pipeline.Steps)
                {
                    await _containerManager.RegisterGroupAsync(new DefaultOperationGroup(step));
                }
            }
            // Check when containers need creating
            // Update current list of containers
            // Should we clean up containers if we are the only boss and just starting?
            // Idea: make configurable if cleanup should occur
            // Idea: make containers have a timeout on heartbeat
            _logger.LogInformation("Begin balancing containers");
            while(!cancelationToken.IsCancellationRequested)
            {
                
                await _containerManager.DiscoverContainersAsync();
                var containerGroups = await _containerManager.GetContainersAsync();
                foreach(var group in containerGroups)
                {
                    // Balance our containers based on configuration
                    // This will also create new containers if some fail
                    await _containerBalancer.BalanceGroupAsync(group);
                }

                await Task.Delay(5000); // TODO: make this configurable
            }

            // Shutdown tasks here
        }
    }
}