using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
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
        private string _currentPipelineHash;
        private string PipelineLocation { 
            get {
                var pipelineLocation = _configuration["Boss:Pipelines:Location"];
                if(!Path.IsPathRooted(pipelineLocation))
                {
                    pipelineLocation = $"{FileUtils.GetBasePath()}/{pipelineLocation}";
                }
                return pipelineLocation;
            }
        }

        private string ScriptsLocation { 
            get {
                var scriptsLocation = _configuration["Boss:Scripts:Location"];
                if(!Path.IsPathRooted(scriptsLocation))
                {
                    scriptsLocation = $"{FileUtils.GetBasePath()}/{scriptsLocation}";
                }
                return scriptsLocation;
            }
        }

        private List<string> AdditionalLocation { 
            get {
                var additionalLocation = new List<string>();
                _configuration.Bind("Boss:Scripts:AdditionalLocations", additionalLocation);
                if(!additionalLocation.Any()) return new List<string>();
                return additionalLocation.Select(x => Path.IsPathRooted(x) ? x : $"{FileUtils.GetBasePath()}/{x}").ToList();
            }
        }

        private List<string> AllScriptLocations {
            get {
                var pipelineLocation = PipelineLocation;
                var scripts = ScriptsLocation;
                var additional = AdditionalLocation;
                var ret = new List<string>() { pipelineLocation, scripts };
                ret.AddRange(additional);
                return ret;
            }
        }

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

            // Check when containers need creating
            // Update current list of containers
            // Should we clean up containers if we are the only boss and just starting?
            // Idea: make configurable if cleanup should occur
            // Idea: make containers have a timeout on heartbeat
            _logger.LogInformation("Begin balancing containers");
            while(!cancelationToken.IsCancellationRequested)
            {
                if(await CheckPipelinesUpdated(AllScriptLocations))
                {
                    _logger.LogInformation("Loading New Pipelines");
                    await _containerManager.RunShutdownAsync();
                    _pipelineManager.ClearPipelines();
                    // For some reason this needs to be run here to work instead of in the PipelineManager
                    for (int i = 0; (i < 10); i++)
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        await Task.Delay(1000);
                    }
                    await LoadPipelines();
                }
                await _containerManager.DiscoverContainersAsync();
                var containerGroups = await _containerManager.GetContainersAsync();
                foreach(var group in containerGroups)
                {
                    if((await group.CurrentSizeAsync()) == 0 && group.Stale)
                    {
                        _containerManager.RemoveGroup(group);
                    }
                    // Balance our containers based on configuration
                    // This will also create new containers if some fail
                    await _containerBalancer.BalanceGroupAsync(group);
                }

                await Task.Delay(200); // TODO: make this configurable
            }
            _logger.LogInformation("Shutting down the Container service");
            await _containerManager.DiscoverContainersAsync();
            var groups = await _containerManager.GetContainersAsync();
            foreach(var group in groups)
            {
                foreach(var container in group)
                {
                    await container.SendShutdownAsync();
                }
            }
        }

        public async Task<bool> CheckPipelinesUpdated(List<string> directories) {
            var hashs = await Task.WhenAll(directories.Select(async x => $"{x}:{await FileUtils.GetDirectoryHash(x)}"));
            var newHash = FileUtils.Sha256Hash(string.Join(',',hashs));
            var change = newHash != _currentPipelineHash;
            _currentPipelineHash = newHash;
            return change;
        }

        public async Task LoadPipelines()
        {
            _pipelineManager.CompilePipelines(PipelineLocation);
            _pipelineManager.RegisterPipelines();
            foreach(var step in FilterSteps(_pipelineManager.Pipelines.SelectMany(x => x.Steps).ToList()))
            {
                await _containerManager.RegisterGroupAsync(new DefaultOperationGroup(step));
            }
        }
    }
}