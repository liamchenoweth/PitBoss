using System.Collections.Generic;

namespace PitBoss
{
    public interface IDistributedRequestManager
    {
        IEnumerable<OperationRequest> GenerateDistributedRequest(PipelineRequest pipelineRequest, OperationResponse request, OperationRequest instigatingRequest, PipelineStep targetStep);
    }
}