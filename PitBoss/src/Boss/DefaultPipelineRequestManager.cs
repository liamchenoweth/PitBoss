using System;
using System.Linq;
using System.Text.Json;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Distributed;
using PitBoss.Extensions;

namespace PitBoss {
    public class DefaultPipelineRequestManager : IPipelineRequestManager {
        private IDistributedService _memoryService;
        private IPipelineManager _pipelineManager;
        private IOperationRequestManager _requestManager;
        private BossContext _db;

        public DefaultPipelineRequestManager(
            IDistributedService memoryService, 
            BossContext db, 
            IPipelineManager pipelineManager,
            IOperationRequestManager requestManager)
        {
            _memoryService = memoryService;
            _pipelineManager = pipelineManager;
            _db = db;
            _requestManager = requestManager;
        }

        public void QueueRequest(PipelineRequest request) 
        {
            var step = _pipelineManager.GetPipeline(request.PipelineName).Steps[0];
            var requestString = $"{DefaultOperationRequestManager.CachePrefix}:{step.Name}";
            var operation = new OperationRequest(request, 0);
            // TODO: set this correctly
            operation.CallbackUri = "localhost:4000";
            operation.Status = RequestStatus.Pending;
            request.Status = RequestStatus.Pending;
            OperationRequest genericOperation = operation;
            if(!string.IsNullOrEmpty(request.Input))
            {
                var stepType = step.GetType().GetGenericArguments()[0];
                var input = JsonSerializer.Deserialize(request.Input, stepType);
                var operationRequestType = typeof(OperationRequest<>).MakeGenericType(stepType);
                genericOperation = (OperationRequest) Activator.CreateInstance(operationRequestType, new object[] {operation, input});
            }
            Console.WriteLine(JsonSerializer.Serialize(genericOperation));
            _db.PipelineRequests.Add(request);
            genericOperation.PipelineId = request.Id;
            _db.SaveChanges();
            _requestManager.QueueRequest(genericOperation);
        }

        public void FinishRequest(OperationResponse response)
        {
            var request = _db.PipelineRequests.Where(x => x.Id == response.PipelineId).FirstOrDefault();
            if(request == null) throw new Exception($"Request with ID {response.PipelineId} not found");
            request.Status = RequestStatus.Complete;
            _db.SaveChanges();
            _memoryService.GetCache().Set($"{DefaultOperationRequestManager.CachePrefix}:{response.PipelineId}", response);
        }

        public IEnumerable<PipelineRequest> RequestsForPipeline(string pipelineName) {
            return _db.PipelineRequests.Where(x => x.PipelineName == pipelineName);
        }

        public IEnumerable<PipelineRequest> PendingRequests() {
            return _db.PipelineRequests.Where(x => x.Status == RequestStatus.Pending);
        }

        public IEnumerable<PipelineRequest> InProgressRequests() {
            return _db.PipelineRequests.Where(x => x.Status == RequestStatus.Executing);
        }

        public IEnumerable<PipelineRequest> CompletedRequests(){ 
            return _db.PipelineRequests.Where(x => x.Status == RequestStatus.Complete);
        }

        public IEnumerable<PipelineRequest> FailedRequests(){ 
            return _db.PipelineRequests.Where(x => x.Status == RequestStatus.Failed);
        }

        public PipelineRequest FindRequest(string requestId)
        {
            return _db.PipelineRequests.Where(x => x.Id == requestId).FirstOrDefault();
        }

        public string GetResponseJson(string requestId)
        {
            var request = FindRequest(requestId);
            if(request.Status != RequestStatus.Complete) return null;
            var json = _memoryService.GetCache().GetString($"{DefaultOperationRequestManager.CachePrefix}:{request.Id}");
            return json;
        }

        public OperationResponse GetResponse(string requestId)
        {
            var request = FindRequest(requestId);
            if(request.Status != RequestStatus.Complete) return null;
            var json = _memoryService.GetCache().GetString($"{DefaultOperationRequestManager.CachePrefix}:{request.Id}");
            var pipelineType = _pipelineManager.GetPipeline(request.PipelineName).Output;
            var respType = pipelineType == null ? typeof(OperationResponse) : typeof(OperationResponse<>).MakeGenericType(new Type[] {pipelineType});
            return (OperationResponse)JsonSerializer.Deserialize(json, respType);
        }
    }
}