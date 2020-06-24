using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;

using PitBoss.Utils;

namespace PitBoss
{
    public class DistributedStepService : IDistributedStepService
    {
        private ILogger<DistributedStepService> _logger;
        private BossContext _context;
        private IPipelineManager _pipelineManager;
        private IOperationRequestManager _operationRequestManager;
        private IPipelineRequestManager _pipelineRequestManager;
        private IConfiguration _configuration;

        public DistributedStepService(
            ILogger<DistributedStepService> logger,
            IPipelineManager pipelineManager,
            IPipelineRequestManager pipelineRequestManager,
            IOperationRequestManager operationRequestManager,
            BossContext bossContext,
            IConfiguration configuration
            )
        {
            _logger = logger;
            _pipelineManager = pipelineManager;
            _operationRequestManager = operationRequestManager;
            _pipelineRequestManager = pipelineRequestManager;
            _context = bossContext;
            _configuration = configuration;
        }

        public async Task MonitorDistributedRequests(CancellationToken cancelationToken)
        {
            // Compile pipelines
            var pipelineLocation = _configuration["Boss:Pipelines:Location"];
            if(!Path.IsPathRooted(pipelineLocation))
            {
                pipelineLocation = $"{FileUtils.GetBasePath()}/{pipelineLocation}";
            }
            await _pipelineManager.CompilePipelinesAsync(pipelineLocation);
            _logger.LogInformation("Begin waiting for distributed requests");
            while(!cancelationToken.IsCancellationRequested)
            {
                var executingRequests = _context.DistributedOperationRequests.Include(x => x.SeedingRequestIds).Where(x => x.Status == RequestStatus.Executing).ToList();
                foreach(var request in executingRequests)
                {
                    _context.Entry(request).Reload();
                    var reloadedRequest = _context.DistributedOperationRequests.Single(x => x.Id == request.Id);
                    var childRequests = _context.OperationRequests.Where(x => x.ParentRequestId == reloadedRequest.Id).ToList();
                    foreach(var childRequest in childRequests) _context.Entry(childRequest).Reload();
                    childRequests = _context.OperationRequests.Where(x => x.ParentRequestId == reloadedRequest.Id).ToList();
                    if(childRequests.Where(x => x.Status == RequestStatus.Failed).Count() > 0){ reloadedRequest.Status = RequestStatus.Failed; reloadedRequest.Completed = DateTime.Now; continue; }
                    if(childRequests.Where(x => x.Status == RequestStatus.Cancelled).Count() > 0){ reloadedRequest.Status = RequestStatus.Cancelled; reloadedRequest.Completed = DateTime.Now; continue; }
                    if(childRequests.Where(x => x.PipelineStepId == reloadedRequest.EndingStepId).Count() < reloadedRequest.SeedingRequestIds.Count()
                        || childRequests.Where(x => x.Status == RequestStatus.Pending).Count() > 0
                        || childRequests.Where(x => x.Status == RequestStatus.Executing).Count() > 0) continue; // We aren't finished yet
                    // We have finished our distributed step
                    var pipeline = _pipelineManager.GetPipeline(reloadedRequest.PipelineName);
                    var pipelineStep = pipeline.Steps.Single(x => x.Id == reloadedRequest.EndingStepId);
                    var outputType = pipelineStep.Output;
                    var results = childRequests
                        .Where(x => x.PipelineStepId == reloadedRequest.EndingStepId)
                        .Join(_context.OperationResponses, x => x.Id, x => x.Id, (x, y) => JsonConvert.DeserializeObject(y.Result, outputType))
                        // This will break dictionaries
                        // TODO: allow for other types of IEnumerable
                        .ToList();
                    var nextStep = pipelineStep.GetNextStep(null); // This may cause issues, but easier for now
                    // TODO: Maybe try to move this to IOperationRequestManager.ProcessResponse
                    var outgoingRequest = (OperationRequest) Activator.CreateInstance(typeof(OperationRequest<>).MakeGenericType(typeof(IEnumerable<>).MakeGenericType(pipelineStep.Output)));
                    outgoingRequest.PipelineId = reloadedRequest.PipelineId;
                    outgoingRequest.PipelineName = reloadedRequest.PipelineName;
                    outgoingRequest.PipelineStepId = nextStep;
                    var pipeRequest = _context.PipelineRequests.Where(x => x.Id == reloadedRequest.PipelineId).FirstOrDefault();
                    var dbRequest = _context.OperationRequests.Single(x => x.Id == reloadedRequest.Id);
                    dbRequest.Status = RequestStatus.Complete;
                    dbRequest.Completed = DateTime.Now;
                    var opResult = new OperationResponse(dbRequest);
                    opResult.Result = JsonConvert.SerializeObject(results);
                    opResult.Success = true;
                    _context.OperationResponses.Add(opResult);
                    _context.SaveChanges();
                    if(string.IsNullOrEmpty(nextStep) || pipeRequest.Status == RequestStatus.Cancelled) {
                        _pipelineRequestManager.FinishRequest(opResult);
                        continue;
                    }
                    Console.WriteLine(outgoingRequest.GetType().GetGenericArguments().First().Name);
                    outgoingRequest
                        .GetType()
                        .GetProperty("Parameter")
                        .GetSetMethod()
                        .Invoke(outgoingRequest, new object[] { typeof(Enumerable)
                            .GetMethod("Cast")
                            .MakeGenericMethod(pipelineStep.Output)
                            .Invoke(null, new object[] {results}) 
                        });
                    _operationRequestManager.QueueRequest(outgoingRequest);
                }
                await Task.Delay(5000);
            }
            _logger.LogInformation("Shutting down the Operation service");
        }
    }
}