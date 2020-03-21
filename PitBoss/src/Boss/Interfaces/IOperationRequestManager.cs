using System.Collections.Generic;

namespace PitBoss {
    public interface IOperationRequestManager
    {
        void QueueRequest(OperationRequest request);
        bool ProcessResponse(OperationResponse response);
        OperationRequest FetchNextRequest(PipelineStep step);
        void ReturnRequest(OperationRequest request);
        IEnumerable<OperationRequest> PendingRequests();
        IEnumerable<OperationRequest> InProgressRequests();
        IEnumerable<OperationRequest> CompletedRequests();
        IEnumerable<OperationRequest> FailedRequests();
        OperationRequest FindRequest(string requestId);
    }
}