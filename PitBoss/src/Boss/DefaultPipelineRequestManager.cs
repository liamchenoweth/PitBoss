using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Distributed;
using PitBoss.Extensions;

namespace PitBoss {
    public class DefaultPipelineRequestManager : IPipelineRequestManager {
        private IDistributedService _memoryService;
        private BossContext _db;

        public DefaultPipelineRequestManager(IDistributedService memoryService, BossContext db)
        {
            _memoryService = memoryService;
            _db = db;
        }

        public void QueueRequest(PipelineRequest request) 
        {
            var requestString = $"{DefaultOperationRequestManager.CachePrefix}:{request.PipelineName}";
            var queue = _memoryService.GetQueue<OperationRequest>(requestString);
            var operation = new OperationRequest(request, 0);
            // TODO: set this correctly
            operation.CallbackUri = "localhost:4000";
            operation.Status = RequestStatus.Pending;
            request.Status = RequestStatus.Pending;
            queue.Push(operation);
            _db.PipelineRequests.Add(request);
            _db.SaveChanges();
        }

        public void FinishRequest(OperationResponse response)
        {
            var request = _db.PipelineRequests.Where(x => x.Id == response.PipelineId).FirstOrDefault();
            if(request == null) throw new Exception($"Request with ID {response.PipelineId} not found");
            request.Status = RequestStatus.Complete;
            _db.SaveChanges();
            _memoryService.GetCache().Set($"{DefaultOperationRequestManager.CachePrefix}:{response.PipelineId}", response);
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
    }
}