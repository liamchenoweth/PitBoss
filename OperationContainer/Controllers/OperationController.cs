using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace PitBoss {
    public class OperationController : Controller {
        public OperationController() {

        }

        [HttpPost("operation/script")]
        public ActionResult RecieveScript(List<IFormFile> executionScript) {

            return Ok();
        }

        [HttpPost("operation/request")]
        public OperationStatus RequestOperation(OperationRequest request) {
            return new OperationStatus();
        }

        [HttpGet("operation/status")]
        public OperationStatus RequestStatus() {
            return new OperationStatus();
        }

        [HttpGet("operation/status")]
        public OperationStatus RequestStatus(string requestId) {
            return new OperationStatus();
        }
    }
}