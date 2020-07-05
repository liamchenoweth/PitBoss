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
        private IBossContextFactory _contextFactory;

        public DefaultPipelineRequestManager(
            IDistributedService memoryService, 
            IBossContextFactory db, 
            IPipelineManager pipelineManager,
            IOperationRequestManager requestManager)
        {
            _memoryService = memoryService;
            _pipelineManager = pipelineManager;
            _contextFactory = db;
            _requestManager = requestManager;
        }

        public void QueueRequest(PipelineRequest request) 
        {
            var pipeline = _pipelineManager.GetPipeline(request.PipelineName);
            request.PipelineVersion = pipeline.Version;
            var step = pipeline.Steps[0];
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
            using(var db = _contextFactory.GetContext())
            {
                db.PipelineRequests.Add(request);
                genericOperation.PipelineId = request.Id;
                db.SaveChanges();
            }
            _requestManager.QueueRequest(genericOperation);
        }

        public void FinishRequest(OperationResponse response)
        {
            using(var db = _contextFactory.GetContext())
            {
                var request = db.PipelineRequests.Where(x => x.Id == response.PipelineId).FirstOrDefault();
                if(request == null) throw new Exception($"Request with ID {response.PipelineId} not found");
                request.Status = response.Success ? RequestStatus.Complete : RequestStatus.Failed;
                request.ResponseId = response.Id;
                db.SaveChanges();
            }
            _memoryService.GetCache().Set($"{DefaultOperationRequestManager.CachePrefix}:{response.PipelineId}", response);
        }

        public IEnumerable<PipelineRequest> RequestsForPipeline(string pipelineName, bool expanded = false) {
            using(var db = _contextFactory.GetContext())
            {
                var requests = db.PipelineRequests.Where(x => x.PipelineName == pipelineName);
                if(expanded)
                {
                    requests = requests.Include(x => x.Response).Include(x => x.CurrentRequest);
                }
                return requests;
            }
        }

        public IEnumerable<PipelineRequest> RequestsForPipelineVersion(string pipelineVersion, bool expanded = false) {
            using(var db = _contextFactory.GetContext())
            {
                var requests = db.PipelineRequests.Where(x => x.PipelineVersion == pipelineVersion);
                if(expanded)
                {
                    requests = requests.Include(x => x.Response).Include(x => x.CurrentRequest);
                }
                return requests;
            }
        }

        public IEnumerable<PipelineRequest> PendingRequests(bool expanded = false) {
            using(var db = _contextFactory.GetContext())
            {
                var requests = db.PipelineRequests.Where(x => x.Status == RequestStatus.Pending);
                if(expanded)
                {
                    requests = requests.Include(x => x.Response).Include(x => x.CurrentRequest);
                }
                return requests;
            }
        }

        public IEnumerable<PipelineRequest> InProgressRequests(bool expanded = false) {
            using(var db = _contextFactory.GetContext())
            {
                var requests = db.PipelineRequests.Where(x => x.Status == RequestStatus.Executing);
                if(expanded)
                {
                    requests = requests.Include(x => x.Response).Include(x => x.CurrentRequest);
                }
                return requests;
            }
        }

        public IEnumerable<PipelineRequest> CompletedRequests(bool expanded = false){ 
            using(var db = _contextFactory.GetContext())
            {
                var requests = db.PipelineRequests.Where(x => x.Status == RequestStatus.Complete);
                if(expanded)
                {
                    requests = requests.Include(x => x.Response).Include(x => x.CurrentRequest);
                }
                return requests;
            }
        }

        public IEnumerable<PipelineRequest> FailedRequests(bool expanded = false){ 
            using(var db = _contextFactory.GetContext())
            {
                var requests = db.PipelineRequests.Where(x => x.Status == RequestStatus.Failed);
                if(expanded)
                {
                    requests = requests.Include(x => x.Response).Include(x => x.CurrentRequest);
                }
                return requests;
            }
        }

        public IEnumerable<PipelineRequest> AllRequests(bool expanded = false){ 
            using(var db = _contextFactory.GetContext())
            {
                IQueryable<PipelineRequest> requests = db.PipelineRequests;
                if(expanded)
                {
                    requests = requests.Include(x => x.Response).Include(x => x.CurrentRequest);
                }
                return requests;
            }
        }

        public PipelineRequest FindRequest(string requestId, bool expanded = false)
        {
            using(var db = _contextFactory.GetContext())
            {
                var requests = db.PipelineRequests.Where(x => x.Id == requestId);
                if(expanded)
                {
                    requests = requests.Include(x => x.Response).Include(x => x.CurrentRequest);
                }
                return requests.ToList().FirstOrDefault();
            }
        }

        public void CancelRequest(string requestId)
        {
            using(var db = _contextFactory.GetContext())
            {
                var request = db.PipelineRequests.Where(x => x.Id == requestId).FirstOrDefault();
                if(request == default) throw new KeyNotFoundException($"request \"{requestId}\" not found");
                request.Status = RequestStatus.Cancelled;
                db.SaveChanges();
            }
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