using System;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace PitBoss {
    public class RequestController : Controller {
        private IPipelineRequestManager _requestManager;
        private IOperationRequestManager _operationManager;
        private ILogger _logger;
        public RequestController(
            IPipelineRequestManager requestManager, 
            IOperationRequestManager operationManager, 
            ILogger<RequestController> logger){
            _requestManager = requestManager;
            _logger = logger;
            _operationManager = operationManager;
        }

        [HttpGet("api/request")]
        public ActionResult GetJob(string requestId)
        {
            var request = _requestManager.FindRequest(requestId);
            if(request == null) return NotFound("No job with that ID");
            return Ok(request);
        }

        [HttpGet("api/requests")]
        public ActionResult GetRequests()
        {
            return Ok(_requestManager.AllRequests());
        }

        [HttpGet("api/requests/{id}")]
        public ActionResult GetRequest(string id)
        {
            var request = _requestManager.FindRequest(id, true);
            if(request == default) return NotFound();
            return Ok(request);
        }

        [HttpGet("api/requests/{id}/operations")]
        public ActionResult GetOperations(string id)
        {
            return Ok(_operationManager.FindOperationsForRequest(id));
        }

        [HttpDelete("api/requests/{id}/cancel")]
        public ActionResult CancelRequest(string id)
        {
            try
            {
                _requestManager.CancelRequest(id);
            }
            catch(KeyNotFoundException)
            {
                return NotFound();
            }
            return Ok();
        }

        [HttpPost("api/request")]
        public ActionResult RequestJob([FromBody]PipelineRequest request) {
            try
            {
                _requestManager.QueueRequest(request);
            }
            catch(JsonException e)
            {
                _logger.LogError(e, "Failed to deserialise request");
                return BadRequest();
            }
            catch(Exception e)
            {
                _logger.LogError(e, "Unknown failure");
                return StatusCode(500);
            }
            return Ok(request);
        }

        [HttpGet("api/result")]
        public ActionResult GetJobResult(string requestId)
        {
            var response = _requestManager.GetResponse(requestId);
            if(response == default) return NotFound();
            return Ok(response);
        }
    }
}