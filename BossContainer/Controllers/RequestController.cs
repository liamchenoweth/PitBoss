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

        [HttpGet("requests")]
        public ActionResult GetRequests()
        {
            return Ok(_requestManager.AllRequests());
        }

        [HttpPost("request")]
        public ActionResult RequestJob([FromBody]PipelineRequest request) {
            _requestManager.QueueRequest(request);
            return Ok(request);
        }

        [HttpGet("result")]
        public ActionResult GetJobResult(string requestId)
        {
            return Ok(_requestManager.GetResponse(requestId));
        }
    }
}