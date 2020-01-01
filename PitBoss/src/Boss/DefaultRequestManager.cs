using System.Collections.Generic;

namespace PitBoss {
    public class DefaultRequestManager : IRequestManager {
        public void ProcessRequest(PipelineRequest request) {

        }

        public PipelineRequest FetchNextRequest(Pipeline pipeline) { 
            return new PipelineRequest();
        }

        public List<PipelineRequest> PendingRequests() {
            return new List<PipelineRequest>();
        }

        public List<PipelineRequest> InProgressRequests() {
            return new List<PipelineRequest>();
        }

        public List<PipelineRequest> CompletedRequests(){ 
            return new List<PipelineRequest>();
        }
    }
}