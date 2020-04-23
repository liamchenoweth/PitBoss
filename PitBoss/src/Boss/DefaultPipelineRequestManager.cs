using System;
using System.Linq;
using System.Text.Json;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.EntityFrameworkCore;
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
            var operation = new OperationRequest(request, step.Id, null);
            request.Status = RequestStatus.Pending;
            OperationRequest genericOperation = operation;
            if(!string.IsNullOrEmpty(request.Input))
            {
                var stepType = step.GetType().GetGenericArguments()[0];
                var input = JsonSerializer.Deserialize(request.Input, stepType);
                var operationRequestType = typeof(OperationRequest<>).MakeGenericType(stepType);
                genericOperation = (OperationRequest) Activator.CreateInstance(operationRequestType, new object[] {operation, input, null});
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
            request.Status = response.Success ? RequestStatus.Complete : RequestStatus.Failed;
            request.Response = response;
            _db.SaveChanges();
            _memoryService.GetCache().Set($"{DefaultOperationRequestManager.CachePrefix}:{response.PipelineId}", response);
        }

        public IEnumerable<PipelineRequest> RequestsForPipeline(string pipelineName, bool expanded = false) {
            var requests = _db.PipelineRequests.Where(x => x.PipelineName == pipelineName);
            if(expanded)
            {
                requests = requests.Include(x => x.Response).Include(x => x.CurrentRequest);
            }
            return requests;
        }

        public IEnumerable<PipelineRequest> PendingRequests(bool expanded = false) {
            var requests = _db.PipelineRequests.Where(x => x.Status == RequestStatus.Pending);
            if(expanded)
            {
                requests = requests.Include(x => x.Response).Include(x => x.CurrentRequest);
            }
            return requests;
        }

        public IEnumerable<PipelineRequest> InProgressRequests(bool expanded = false) {
            var requests = _db.PipelineRequests.Where(x => x.Status == RequestStatus.Executing);
            if(expanded)
            {
                requests = requests.Include(x => x.Response).Include(x => x.CurrentRequest);
            }
            return requests;
        }

        public IEnumerable<PipelineRequest> CompletedRequests(bool expanded = false){ 
            var requests = _db.PipelineRequests.Where(x => x.Status == RequestStatus.Complete);
            if(expanded)
            {
                requests = requests.Include(x => x.Response).Include(x => x.CurrentRequest);
            }
            return requests;
        }

        public IEnumerable<PipelineRequest> FailedRequests(bool expanded = false){ 
            var requests = _db.PipelineRequests.Where(x => x.Status == RequestStatus.Failed);
            if(expanded)
            {
                requests = requests.Include(x => x.Response).Include(x => x.CurrentRequest);
            }
            return requests;
        }

        public IEnumerable<PipelineRequest> AllRequests(bool expanded = false){ 
            IQueryable<PipelineRequest> requests = _db.PipelineRequests;
            if(expanded)
            {
                requests = requests.Include(x => x.Response).Include(x => x.CurrentRequest);
            }
            return requests;
        }

        public PipelineRequest FindRequest(string requestId, bool expanded = false)
        {
            var requests = _db.PipelineRequests.Where(x => x.Id == requestId);
            if(expanded)
            {
                requests = requests.Include(x => x.Response).Include(x => x.CurrentRequest);
            }
            return requests.ToList().FirstOrDefault();
        }

        public void CancelRequest(string requestId)
        {
            var request = _db.PipelineRequests.Where(x => x.Id == requestId).FirstOrDefault();
            if(request == default) throw new KeyNotFoundException($"request \"{requestId}\" not found");
            request.Status = RequestStatus.Cancelled;
            _db.SaveChanges();
        }

        public string GetResponseJson(string requestId)
        {
            var request = FindRequest(requestId, true);
            if(request.Status != RequestStatus.Complete) return null;
            var json = _memoryService.GetCache().GetString($"{DefaultOperationRequestManager.CachePrefix}:{request.Id}");
            if(json != null)
            {
                return json;
            }
            return request.Response.Result;
        }

        public OperationResponse GetResponse(string requestId)
        {
            var request = FindRequest(requestId, true);
            if(request.Status != RequestStatus.Complete) return null;
            var json = _memoryService.GetCache().GetString($"{DefaultOperationRequestManager.CachePrefix}:{request.Id}");
            var pipelineType = _pipelineManager.GetPipeline(request.PipelineName).Output;
            var respType = pipelineType == null ? typeof(OperationResponse) : typeof(OperationResponse<>).MakeGenericType(new Type[] {pipelineType});
            if(json != null)
            {
                return (OperationResponse)JsonSerializer.Deserialize(json, respType);
            }
            return (OperationResponse)Activator.CreateInstance(respType, new object[]{request.Response});
        }
    }
}