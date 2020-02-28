using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Distributed;

namespace PitBoss {
    public class DefaultOperationRequestManager : IOperationRequestManager {
        private IDistributedService _memoryService;
        private IPipelineManager _pipelineManager;
        private IPipelineRequestManager _pipelineRequestManager;
        public const string CachePrefix = "operation-request";
        private BossContext _db;

        public DefaultOperationRequestManager(
            IDistributedService memoryService, 
            BossContext db, 
            IPipelineManager pipelineManager, 
            IPipelineRequestManager pipelineRequestManager)
        {
            _memoryService = memoryService;
            _pipelineManager = pipelineManager;
            _pipelineRequestManager = pipelineRequestManager;
            _db = db;
        }

        public void QueueRequest(OperationRequest request) 
        {
            var requestString = $"{CachePrefix}:{request.PipelineName}";
            var queue = _memoryService.GetQueue<OperationRequest>(requestString);
            queue.Push(request);
            request.Status = RequestStatus.Pending;
            _db.OperationRequests.Add(request);
            _db.SaveChanges();
        }

        public void ProcessResponse(OperationResponse response)
        {
            var respType = response.GetType().GenericTypeArguments[0];
            var request = (OperationRequest) Activator.CreateInstance(typeof(OperationRequest).MakeGenericType(new Type[]{respType}));
            request.PipelineId = response.PipelineId;
            request.PipelineName = response.PipelineName;
            var pipeline = _pipelineManager.Pipelines.Where(x => x.Name == response.PipelineName).FirstOrDefault();
            if(pipeline == null) throw new Exception($"Pipeline {response.PipelineName} not found");
            if(pipeline.Steps.Count <= response.PipelineStepId + 1)
            {
                _pipelineRequestManager.FinishRequest(response);
                return;
            }
            var nextStep = pipeline.Steps[response.PipelineStepId + 1];
            var resp = response.GetType().GetProperty("Result").GetGetMethod().Invoke(response, null);
            request.GetType().GetProperty("Parameter").GetSetMethod().Invoke(request, new object[] { resp });
            QueueRequest(request);
        }

        public OperationRequest FetchNextRequest(PipelineStep Operation) { 
            var requestString = $"{CachePrefix}:{Operation.Name}";
            var queue = _memoryService.GetQueue<OperationRequest>(requestString);
            var item = queue.Pop();
            if(item == null) return null;
            var dbItem = _db.OperationRequests.Where(x => x.Id == item.Id).FirstOrDefault();
            if(dbItem != null)
            {
                dbItem.Status = RequestStatus.Executing;
                _db.SaveChanges();
            }
            return item;
        }

        public void ReturnRequest(OperationRequest request)
        {
            var step = _pipelineManager.GetPipeline(request.PipelineName).Steps[request.PipelineStepId];
            var requestString = $"{CachePrefix}:{step.Name}";
            var queue = _memoryService.GetQueue<OperationRequest>(requestString);
            queue.PushFront(request);
            var dbRequest = _db.OperationRequests.Where(x => x.Id == request.Id).FirstOrDefault();
            if(dbRequest == null) return;
            dbRequest.Status = RequestStatus.Pending;
            _db.SaveChanges();
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
    }
}