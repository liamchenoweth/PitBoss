using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.IO;
using System;
using PitBoss.Utils;

namespace PitBoss {
    public class OperationGroupService : IOperationGroupService {

        private IPipelineManager _pipelineManager;
        private IConfiguration _configuration;
        private IContainerManager _containerManager;
        private IOperationRequestManager _operationRequestManager;
        private ILogger<OperationGroupService> _logger;
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

        public OperationGroupService(
            IPipelineManager pipelineManager, 
            IConfiguration configuration, 
            IContainerManager containerManager,
            IOperationRequestManager operationRequestManager,
            ILogger<OperationGroupService> logger) 
        {
            _pipelineManager = pipelineManager;
            _configuration = configuration;
            _containerManager = containerManager;
            _operationRequestManager = operationRequestManager;
            _logger = logger;
        }

        public async Task MonitorRequests(CancellationToken cancelationToken)
        {
            // Compile pipelines

            while(!cancelationToken.IsCancellationRequested)
            {
                if(await CheckPipelinesUpdated(AllScriptLocations))
                {
                    await LoadPipelines();
                }
                // Just keep polling, polling, polling...
                foreach(var pipeline in _pipelineManager.Pipelines)
                {
                    foreach(var step in pipeline.Steps)
                    {
                        // Notes on process
                        // Change requests -> change from 1 request = 1 pipeline to 1 request = 1 operation, that then leads to next
                        // ie completing pipeline through linking operations
                        // 1. Formulate new request (where are we in the pipeline, what do we need to do next)
                        // 2. Balance requests to pods for processing (Shouldn't be bombarding a single pod, could do this with random distribution, or regular polling)
                        // 3. When work completes, worker calls back and we place next operation on stack
                        // - this means the request to worker needs either information on its current place in the pipeline, the next operation or both

                        // Execute new task
                        // Mark tasks as complete
                        // Poll for new tasks
                        var request = _operationRequestManager.FetchNextRequest(step);
                        if(request == null) continue;
                        if(request.RetryCount > 0)
                        {
                            var retryStrategy = pipeline.Description.RetryStrategy;
                            if(step.RetryStrategy != RetryStrategy.Inherit) retryStrategy = step.RetryStrategy;
                            var backoffSeconds = _configuration.GetValue<int>("OperationGroupContainer:Retry:Backoff");
                            switch(retryStrategy)
                            {
                                case RetryStrategy.Linear:
                                    if(request.Queued.AddSeconds(backoffSeconds * request.RetryCount) < DateTime.Now)
                                    {
                                        _operationRequestManager.ReturnRequest(request, true);
                                        continue;
                                    }
                                    break;
                                case RetryStrategy.Exponential:
                                    if(request.Queued.AddSeconds(backoffSeconds * request.RetryCount) < DateTime.Now)
                                    {
                                        _operationRequestManager.ReturnRequest(request, true);
                                        continue;
                                    }
                                    break;
                                default:
                                    continue;
                            }
                        }

                        var group = await _containerManager.GetContainersByStepAsync(step);
                        try
                        {
                            await group.SendRequestAsync(request);
                        }
                        catch(ContainerUnavailableException e)
                        {
                            _logger.LogWarning(e, "Container unavailable for processing, placing request back on stack");
                            _operationRequestManager.ReturnRequest(request);
                            continue;
                        }
                    }
                }
                await Task.Delay(100);
            }
            _logger.LogInformation("Shutting down the Operation service");
            // Do our shutdown tasks
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
        }
    }
}