using System.Collections.Generic;

namespace PitBoss {
    public interface IRequestManager
    {
        void ProcessRequest(JobRequest request);
        JobRequest FetchNextRequest(Pipeline pipeline);
        List<JobRequest> PendingRequests();
        List<JobRequest> InProgressRequests();
        List<JobRequest> CompletedRequests();
    }
}