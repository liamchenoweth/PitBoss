using System.Collections.Generic;

namespace PitBoss {
    public interface IPipelineRequestManager
    {
        void QueueRequest(PipelineRequest request);
        void FinishRequest(OperationResponse response);
        IEnumerable<PipelineRequest> RequestsForPipeline(string pipelineName, bool expanded = false);
        IEnumerable<PipelineRequest> RequestsForPipelineVersion(string pipelineVersion, bool expanded = false);
        IEnumerable<PipelineRequest> PendingRequests(bool expanded = false);
        IEnumerable<PipelineRequest> InProgressRequests(bool expanded = false);
        IEnumerable<PipelineRequest> CompletedRequests(bool expanded = false);
        IEnumerable<PipelineRequest> FailedRequests(bool expanded = false);
        IEnumerable<PipelineRequest> AllRequests(bool expanded = false);
        PipelineRequest FindRequest(string requestId, bool expanded = false);
        void CancelRequest(string requestId);
        string GetResponseJson(string requestId);
        OperationResponse GetResponse(string requestId);
    }
}