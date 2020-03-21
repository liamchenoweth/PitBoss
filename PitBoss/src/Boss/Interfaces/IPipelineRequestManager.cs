using System.Collections.Generic;

namespace PitBoss {
    public interface IPipelineRequestManager
    {
        void QueueRequest(PipelineRequest request);
        void FinishRequest(OperationResponse response);
        IEnumerable<PipelineRequest> RequestsForPipeline(string pipelineName);
        IEnumerable<PipelineRequest> PendingRequests();
        IEnumerable<PipelineRequest> InProgressRequests();
        IEnumerable<PipelineRequest> CompletedRequests();
        IEnumerable<PipelineRequest> FailedRequests();
        PipelineRequest FindRequest(string requestId);
        string GetResponseJson(string requestId);
        OperationResponse GetResponse(string requestId);
    }
}