using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace PitBoss
{
    public class DefaultDistributedRequestManager : IDistributedRequestManager
    {
        private IPipelineManager _pipelineManager;
        private IBossContextFactory _contextFactory;

        public DefaultDistributedRequestManager(
            IPipelineManager pipelineManager,
            IBossContextFactory context)
        {
            _pipelineManager = pipelineManager;
            _contextFactory = context;
        }

        public IEnumerable<OperationRequest> GenerateDistributedRequest(
            PipelineRequest pipeRequest,
            OperationResponse response, 
            OperationRequest instigatingRequest, 
            PipelineStep targetStep)
        {
            var requestType = targetStep.Input;
            var resultValue = (IEnumerable) response
                .GetType()
                .GetProperties()
                .Single(x => x.Name == "Result" && x.DeclaringType == response.GetType())
                .GetMethod.Invoke(response, null);
            var distributedRequest = new DistributedOperationRequest(pipeRequest, targetStep.Id, targetStep.DistributedEndId, instigatingRequest);
            distributedRequest.Status = RequestStatus.Executing;
            using(var context = _contextFactory.GetContext())
            {
                context.DistributedOperationRequests.Add(distributedRequest);
                context.SaveChanges();
                var requests = resultValue.Cast<object>().Select(x => {
                    var request = new OperationRequest(pipeRequest, targetStep.Id, distributedRequest);
                    request.CallbackUri = instigatingRequest.CallbackUri;
                    request.ParentRequestId = distributedRequest.Id;
                    var operationRequestType = typeof(OperationRequest<>).MakeGenericType(new Type[] {requestType});
                    var properRequest = (OperationRequest) Activator.CreateInstance(operationRequestType, new object[] {request, x, instigatingRequest});
                    return properRequest;
                }).ToList();
                distributedRequest.SeedingRequestIds = requests.Select(x => new DistributedRequestSeed { OperationRequest = x, DistributedOperationRequest = distributedRequest }).ToList();
                context.SaveChanges();
                return requests;
            }
        }
    }
}