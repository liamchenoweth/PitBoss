using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;

namespace PitBoss {
    public class DefaultOperationRequestManager : IOperationRequestManager {
        private IDistributedService _memoryService;
        private IPipelineManager _pipelineManager;
        private IConfiguration _configuration;
        private IDistributedRequestManager _distributedRequestManager;
        //private IPipelineRequestManager _pipelineRequestManager;
        public const string CachePrefix = "operation-request";
        private BossContext _db;

        public DefaultOperationRequestManager(
            IDistributedService memoryService, 
            BossContext db, 
            IPipelineManager pipelineManager,
            IConfiguration configuration,
            IDistributedRequestManager distributedRequestManager
            )
        {
            _memoryService = memoryService;
            _pipelineManager = pipelineManager;
            _db = db;
            _configuration = configuration;
            _distributedRequestManager = distributedRequestManager;
        }

        public void QueueRequest(OperationRequest request) 
        {
            var pipelineStep = _pipelineManager.Pipelines.Where(x => x.Name == request.PipelineName).FirstOrDefault()?.Steps.Single(x => x.Id == request.PipelineStepId);
            if(pipelineStep == null) throw new Exception($"No pipeline found by name {request.PipelineName}");
            var requestString = $"{CachePrefix}:{pipelineStep.Name}";
            var queue = _memoryService.GetQueue<OperationRequest>(requestString);
            request.Status = RequestStatus.Pending;
            request.CallbackUri = $"{_configuration["Boss:Callback:Scheme"]}://{_configuration["Boss:Callback:Uri"]}";
            if(!_db.OperationRequests.Contains(request))
            {
                _db.OperationRequests.Add(request);
            }
            _db.SaveChanges();
            queue.Push(request);
            SetActiveOperation(request);
        }

        public void SetActiveOperation(OperationRequest request)
        {
            var pipeRequest = _db.PipelineRequests.Where(x => x.Id == request.PipelineId).FirstOrDefault();
            if(pipeRequest != default)
            {
                pipeRequest.CurrentRequest = request;
                pipeRequest.Status = RequestStatus.Executing;
                _db.SaveChanges();
            }
        }

        public bool ProcessResponse(OperationResponse response)
        {
            _db.OperationResponses.Add(response);
            _db.SaveChanges();
            var pipeline = _pipelineManager.GetPipeline(response.PipelineName);
            var nextStep = pipeline.Steps.Single(x => x.Id == response.PipelineStepId).GetNextStep(response);
            if(pipeline == null) throw new Exception($"Pipeline {response.PipelineName} not found");
            var pipeRequest = _db.PipelineRequests.Where(x => x.Id == response.PipelineId).FirstOrDefault();
            var dbRequest = _db.OperationRequests.Single(x => x.Id == response.Id);
            dbRequest.Status = response.Success ? RequestStatus.Complete : RequestStatus.Failed;
            dbRequest.Completed = DateTime.Now;
            _db.SaveChanges();
            Console.WriteLine(dbRequest.Status);
            if(pipeline.Steps.Single(x => x.Id == response.PipelineStepId).IsDistributedEnd) return false;
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
            var resp = response.GetType().GetProperties().Single(x => x.Name == "Result" && x.DeclaringType == response.GetType()).GetGetMethod().Invoke(response, null);
            request.GetType().GetProperty("Parameter").GetSetMethod().Invoke(request, new object[] { resp });
            QueueRequest(request);
            return false;
        }

        public OperationRequest FetchNextRequest(PipelineStep Operation) { 
            var requestString = $"{CachePrefix}:{Operation.Name}";
            Type requestType = typeof(OperationRequest<>).MakeGenericType(new Type[] { Operation.GetType().GenericTypeArguments.First() });
            var queue = _memoryService.GetQueue(requestString, requestType);
            var item = queue.PopObject() as OperationRequest;
            if(item == null) return null;
            var dbItem = _db.OperationRequests.Where(x => x.Id == item.Id).FirstOrDefault();
            if(dbItem != null)
            {
                dbItem.Status = RequestStatus.Executing;
                dbItem.Started = DateTime.Now;
                _db.SaveChanges();
            }
            return item;
        }

        public void ReturnRequest(OperationRequest request)
        {
            var step = _pipelineManager.GetPipeline(request.PipelineName).Steps.Single(x => x.Id == request.PipelineStepId);
            var requestString = $"{CachePrefix}:{step.Name}";
            var queue = _memoryService.GetQueue<OperationRequest>(requestString);
            queue.PushFront(request);
            var dbRequest = _db.OperationRequests.Where(x => x.Id == request.Id).FirstOrDefault();
            if(dbRequest == null) return;
            dbRequest.Status = RequestStatus.Pending;
            dbRequest.Started = default;
            try
            {
                Console.WriteLine("test");
                _db.SaveChanges();
            }catch(Exception e)
            {
                Console.WriteLine("test2");
            }
        }

        public IEnumerable<OperationRequest> PendingRequests() {
            return _db.OperationRequests.Where(x => x.Status == RequestStatus.Pending);
        }

        public IEnumerable<OperationRequest> InProgressRequests() {
            return _db.OperationRequests.Where(x => x.Status == RequestStatus.Executing);
        }

        public IEnumerable<OperationRequest> CompletedRequests(){ 
            return _db.OperationRequests.Where(x => x.Status == RequestStatus.Complete);
        }

        public IEnumerable<OperationRequest> FailedRequests(){ 
            return _db.OperationRequests.Where(x => x.Status == RequestStatus.Failed);
        }

        public OperationRequest FindRequest(string requestId)
        {
            return _db.OperationRequests.Where(x => x.Id == requestId).FirstOrDefault();
        }

        public IEnumerable<OperationRequest> FindOperationsForRequest(string requestId)
        {
            return _db.OperationRequests.Where(x => x.PipelineId == requestId);
        }
    }
}