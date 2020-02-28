using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PitBoss
{
    public class OperationService : BackgroundService
    {
        private IOperationManager _operationManager;
        private IOperationHealthManager _healthManager;
        private ILogger _logger;
        public OperationService(IOperationManager operationManager, ILogger<OperationService> logger, IOperationHealthManager healthManager)
        {
            _operationManager = operationManager;
            _healthManager = healthManager;
            _logger = logger;
        }

        protected async override Task ExecuteAsync(CancellationToken cancelationToken)
        {
            _logger.LogInformation("Begining operation container");
            while(!_operationManager.Ready)
            {
                _logger.LogInformation("Waiting for operation to be sent from boss");
                await Task.Delay(5000);
            }
            while(!cancelationToken.IsCancellationRequested)
            {
                var nextRequest = _operationManager.GetNextRequest();
                if(nextRequest == null)
                {
                    _logger.LogInformation("No operation found, waiting for next request");
                    await Task.Delay(5000);
                    continue;
                }
                try
                {
                    _logger.LogInformation($"Starting request {nextRequest.Id}");
                    var ret = await _operationManager.ProcessRequest(nextRequest);
                    await _operationManager.FinishRequestAsync(nextRequest, ret);
                    _logger.LogInformation($"Finished request {nextRequest.Id}");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to process {nextRequest.Id}");
                    _healthManager.FailActiveOperation(nextRequest, e);
                    await _operationManager.FinishRequestAsync(nextRequest, null);
                }
            }
            _logger.LogInformation("Shutting down operation container");
            _healthManager.SetContainerShutdown();
        }
    }
}