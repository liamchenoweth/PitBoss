using System.Collections.Generic;

namespace PitBoss {
    public interface IRequestManager
    {
        void ProcessRequest(PipelineRequest request);
        PipelineRequest FetchNextRequest(Pipeline pipeline);
        List<PipelineRequest> PendingRequests();
        List<PipelineRequest> InProgressRequests();
        List<PipelineRequest> CompletedRequests();
    }
}