using System;
using Microsoft.AspNetCore.Mvc;

namespace PitBoss {
    public class RequestController : Controller {
        private IPipelineRequestManager _requestManager;
        public RequestController(IPipelineRequestManager requestManager){
            _requestManager = requestManager;
        }

        [HttpGet("request")]
        public ActionResult GetJob(string requestId)
        {
            var request = _requestManager.FindRequest(requestId);
            if(request == null) return NotFound("No job with that ID");
            return Ok(request);
        }

        [HttpPost("request")]
        public ActionResult RequestJob(string pipelineName) {
            var request = new PipelineRequest
            {
                PipelineName = pipelineName
            };

            _requestManager.QueueRequest(request);
            return Ok(request);
        }
    }
}