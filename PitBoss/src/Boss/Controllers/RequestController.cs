using Microsoft.AspNetCore.Mvc;

namespace PitBoss {
    public class RequestController : Controller {
        private IRequestManager _requestManager;
        public RequestController(IRequestManager requestManager){
            _requestManager = requestManager;
        }

        [HttpPost("request")]
        public JobRequest RequestJob(string pipelineName) {
            return new JobRequest();
        }
    }
}