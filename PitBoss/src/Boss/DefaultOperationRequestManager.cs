using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PitBoss {
    public class DefaultOperationRequestManager : IOperationRequestManager {
        private IDistributedService _memoryService;
        private IPipelineManager _pipelineManager;
        private IConfiguration _configuration;
        private IDistributedRequestManager _distributedRequestManager;
        //private IPipelineRequestManager _pipelineRequestManager;
        public const string CachePrefix = "operation-request";
        private IBossContextFactory _contextFactory;
        private ILogger _logger;

        public DefaultOperationRequestManager(
            IDistributedService memoryService, 
            IBossContextFactory db, 
            IPipelineManager pipelineManager,
            IConfiguration configuration,
            IDistributedRequestManager distributedRequestManager,
            ILogger<IOperationRequestManager> logger
            )
        {
            _memoryService = memoryService;
            _pipelineManager = pipelineManager;
            _contextFactory = db;
            _configuration = configuration;
            _distributedRequestManager = distributedRequestManager;
            _logger = logger;
        }

        public void QueueRequest(OperationRequest request) 
        {
            var pipelineStep = _pipelineManager.Pipelines.Where(x => x.Name == request.PipelineName).FirstOrDefault()?.Steps.Single(x => x.Id == request.PipelineStepId);
            if(pipelineStep == null) throw new Exception($"No pipeline found by name {request.PipelineName}");
            var requestString = $"{CachePrefix}:{pipelineStep.Name}";
            var queue = _memoryService.GetQueue<OperationRequest>(requestString);
            request.Queued = DateTime.Now;
            request.Status = RequestStatus.Pending;
            request.CallbackUri = $"{_configuration["Boss:Host:Scheme"]}://{_configuration["Boss:Host:Uri"]}:{_configuration["Boss:Host:Port"]}";
            using(var db = _contextFactory.GetContext())
            {
                if(!db.OperationRequests.Contains(request))
                {
                    db.OperationRequests.Add(request);
                }
                db.SaveChanges();
            }
            queue.Push(request);
            SetActiveOperation(request);
        }

        public void SetActiveOperation(OperationRequest request)
        {
            using(var db = _contextFactory.GetContext())
            {
                var pipeRequest = db.PipelineRequests.Where(x => x.Id == request.PipelineId).FirstOrDefault();
                if(pipeRequest != default)
                {
                    pipeRequest.CurrentRequest = request;
                    pipeRequest.Status = RequestStatus.Executing;
                    db.SaveChanges();
                }
            }
        }

        public bool ProcessResponse(OperationResponse response)
        {
            using(var db = _contextFactory.GetContext())
            {
                if(db.OperationResponses.SingleOrDefault(x => x.Id == response.Id) != default)
                {
                    db.Entry(response);
                }
                else
                {
                    db.OperationResponses.Add(response);
                }
                db.SaveChanges();
                var pipeline = _pipelineManager.GetPipeline(response.PipelineName);
                var currentStep = pipeline.Steps.Single(x => x.Id == response.PipelineStepId);
                var nextStep = currentStep.GetNextStep(response);
                if(pipeline == null) throw new Exception($"Pipeline {response.PipelineName} not found");
                var pipeRequest = db.PipelineRequests.Where(x => x.Id == response.PipelineId).FirstOrDefault();
                var dbRequest = db.OperationRequests.Single(x => x.Id == response.Id);
                if(!response.Success && dbRequest.RetryCount < 5)
                {
                    _logger.LogWarning($"Operation: {response.Id} has failed.");
                    var retryStrategy = pipeline.Description.RetryStrategy;
                    if(currentStep.RetryStrategy != RetryStrategy.Inherit) retryStrategy = currentStep.RetryStrategy;
                    dbRequest.Status = RequestStatus.Pending;
                    dbRequest.RetryCount += 1;
                    QueueRequest(dbRequest);
                    db.SaveChanges();
                    return false;
                }
                dbRequest.Status = response.Success ? RequestStatus.Complete : RequestStatus.Failed;
                dbRequest.Completed = DateTime.Now;
                db.SaveChanges();
                if(currentStep.IsDistributedEnd) return false;
                if(string.IsNullOrEmpty(nextStep) || !response.Success || pipeRequest.Status == RequestStatus.Cancelled)
                {
                    return true;
                }
                var nextStepObject = pipeline.Steps.Single(x => x.Id == nextStep);
                if(nextStepObject.IsDistributedStart)
                {
                    var distributedRequests = _distributedRequestManager.GenerateDistributedRequest(pipeRequest, response, FindRequest(response.Id), nextStepObject);
                    foreach(var newRequest in distributedRequests)
                    {
                        QueueRequest(newRequest);
                    }
                    return false;
                }
                var respType = response.GetType().GenericTypeArguments[0];
                var request = (OperationRequest) Activator.CreateInstance(typeof(OperationRequest<>).MakeGenericType(new Type[]{respType}));
                request.PipelineId = response.PipelineId;
                request.PipelineName = response.PipelineName;
                request.PipelineStepId = nextStep;
                request.ParentRequestId = dbRequest.ParentRequestId;
                request.InstigatingRequestId = dbRequest.Id;
                var resp = response.GetType().GetProperties().Single(x => x.Name == "Result" && x.DeclaringType == response.GetType()).GetGetMethod().Invoke(response, null);
                request.GetType().GetProperty("Parameter").GetSetMethod().Invoke(request, new object[] { resp });
                QueueRequest(request);
                return false;
            }
        }

        public OperationRequest FetchNextRequest(PipelineStep Operation) { 
            var requestString = $"{CachePrefix}:{Operation.Name}";
            Type requestType = typeof(OperationRequest<>).MakeGenericType(new Type[] { Operation.GetType().GenericTypeArguments.First() });
            var queue = _memoryService.GetQueue(requestString, requestType);
            var item = queue.PopObject() as OperationRequest;
            if(item == null) return null;
            using(var db = _contextFactory.GetContext())
            {
                var dbItem = db.OperationRequests.Where(x => x.Id == item.Id).FirstOrDefault();
                if(dbItem != null)
                {
                    dbItem.Status = RequestStatus.Executing;
                    dbItem.Started = DateTime.Now;
                    db.SaveChanges();
                }
            }
            return item;
        }

        public void ReturnRequest(OperationRequest request, bool back = false)
        {
            var step = _pipelineManager.GetPipeline(request.PipelineName).Steps.Single(x => x.Id == request.PipelineStepId);
            var requestString = $"{CachePrefix}:{step.Name}";
            var queue = _memoryService.GetQueue<OperationRequest>(requestString);
            if(!back)
            {
                queue.PushFront(request);
            }
            else
            {
                queue.Push(request);
            }
            using(var db = _contextFactory.GetContext())
            {
                var dbRequest = db.OperationRequests.Where(x => x.Id == request.Id).FirstOrDefault();
                if(dbRequest == null) return;
                dbRequest.Status = RequestStatus.Pending;
                dbRequest.Started = default;
                db.SaveChanges();
            }
        }

        public IEnumerable<OperationRequest> PendingRequests() {
            using(var db = _contextFactory.GetContext())
            {
                return db.OperationRequests.Where(x => x.Status == RequestStatus.Pending);
            }
        }

        public IEnumerable<OperationRequest> InProgressRequests() {
            using(var db = _contextFactory.GetContext())
            {
                return db.OperationRequests.Where(x => x.Status == RequestStatus.Executing);
            }
        }

        public IEnumerable<OperationRequest> CompletedRequests(){
            using(var db = _contextFactory.GetContext())
            { 
                return db.OperationRequests.Where(x => x.Status == RequestStatus.Complete);
            }
        }

        public IEnumerable<OperationRequest> FailedRequests(){
            using(var db = _contextFactory.GetContext())
            { 
                return db.OperationRequests.Where(x => x.Status == RequestStatus.Failed);
            }
        }

        public OperationRequest FindRequest(string requestId)
        {
            using(var db = _contextFactory.GetContext())
            {
                return db.OperationRequests.Where(x => x.Id == requestId).FirstOrDefault();
            }
        }

        public IEnumerable<OperationRequest> FindOperationsForRequest(string requestId)
        {
            using(var db = _contextFactory.GetContext())
            {
                return db.OperationRequests.Where(x => x.PipelineId == requestId);
            }
        }
    }
}