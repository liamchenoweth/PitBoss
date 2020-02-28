using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;

namespace PitBoss {
    public class OperationGroupService : BackgroundService {

        private IPipelineManager _pipelineManager;
        private IConfiguration _configuration;
        private IContainerManager _containerManager;
        private IOperationRequestManager _operationRequestManager;
        private ILogger<OperationGroupService> _logger;

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

        protected async override Task ExecuteAsync(CancellationToken cancelationToken)
        {
            // Compile pipelines
            await _pipelineManager.CompilePipelinesAsync(_configuration["Boss:Pipelines:Location"]);
            // Poll for new containers
            // await _containerManager.DiscoverContainersAsync();
            // wait for ContainerService to discover containers
            while(!_containerManager.Ready)
            {
                _logger.LogInformation("Waiting for containers to be discovered");
                await Task.Delay(5000); // TODO: Maybe make this configurable
            }
            _logger.LogInformation("Begin waiting for incoming requests");
            while(!cancelationToken.IsCancellationRequested)
            {
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
                await Task.Delay(5000);
            }
            _logger.LogInformation("Shutting down the Operation service");
            // Do our shutdown tasks
        }
    }
}