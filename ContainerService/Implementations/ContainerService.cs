using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using PitBoss.Utils;
using PitBoss.Extensions;

namespace PitBoss
{
    public class ContainerService : IContainerService
    {
        private IContainerManager _containerManager;
        private IContainerBalancer _containerBalancer;
        private IPipelineManager _pipelineManager;
        private ILogger<ContainerService> _logger;
        private IConfiguration _configuration;

        public ContainerService(
            ILogger<ContainerService> logger,
            IContainerManager containerManager,
            IContainerBalancer containerBalancer,
            IPipelineManager pipelineManager,
            IConfiguration configuration)
        {
            _logger = logger;
            _containerManager = containerManager;
            _containerBalancer = containerBalancer;
            _pipelineManager = pipelineManager;
            _configuration = configuration;
        }

        private List<PipelineStep> FilterSteps(List<PipelineStep> steps)
        {
            var outputSteps = new List<PipelineStep>();
            foreach(var stepGroup in steps.GroupBy(x => x.Name))
            {
                switch(_configuration["Boss:Pipelines:ContainerCountAggregator"])
                {
                    case "Minimum":
                        outputSteps.Add(stepGroup.MinBy(x => x.TargetCount));
                        break;
                    case "Maximum":
                        outputSteps.Add(stepGroup.MaxBy(x => x.TargetCount));
                        break;
                    case "Median":
                        outputSteps.Add(stepGroup.MedianBy(x => x.TargetCount));
                        break;
                }
            }
            return outputSteps;
        }

        public async Task BalanceContainers(CancellationToken cancelationToken)
        {
            // Compile pipelines
            var pipelineLocation = _configuration["Boss:Pipelines:Location"];
            if(!Path.IsPathRooted(pipelineLocation))
            {
                pipelineLocation = $"{FileUtils.GetBasePath()}/{pipelineLocation}";
            }
            await _pipelineManager.CompilePipelinesAsync(pipelineLocation);

            foreach(var step in FilterSteps(_pipelineManager.Pipelines.SelectMany(x => x.Steps).ToList()))
            {
                await _containerManager.RegisterGroupAsync(new DefaultOperationGroup(step));
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
            _logger.LogInformation("Shutting down the Container service");
            // Shutdown tasks here
        }
    }
}