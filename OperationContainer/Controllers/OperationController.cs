using System;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace PitBoss {
    public class OperationController : Controller {
        private IWebHostEnvironment _host;
        private IOperationManager _operationManager;
        private IOperationHealthManager _healthManager;
        public OperationController(
            IWebHostEnvironment host, 
            IOperationManager operationManager, 
            IOperationHealthManager healthManager
        ) {
            _host = host;
            _operationManager = operationManager;
            _healthManager = healthManager;
        }

        [HttpPost("operation/script")]
        public async Task<ActionResult> RecieveScript(List<IFormFile> executionScript) 
        {
            var baseLocation = "OperationContainer/scripts";
            Directory.CreateDirectory(baseLocation);
            foreach(var file in executionScript)
            {
                if(file.Length > 0)
                {
                    using(var fileStream = new FileStream($"{baseLocation}/{file.FileName}", FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                    await _operationManager.CompileOperationAsync($"{baseLocation}/{file.FileName}");
                }
            }
            return Ok();
        }

        [HttpPost("operation/request")]
        public OperationRequestStatus RequestOperation([FromBody]JObject jrequest) 
        {
            var requestType = typeof(OperationRequest<>).MakeGenericType(new Type[] {_operationManager.InputType});
            var request = (OperationRequest)jrequest.ToObject(requestType);
            _operationManager.QueueRequest(request);
            return _healthManager.GetOperationStatus(request);
        }

        [HttpGet("operation/status")]
        public ActionResult RequestStatus(string requestId) 
        {
            if(requestId == null) return BadRequest("No request id given");
            var status = _healthManager.GetOperationStatus(requestId);
            if(status == null) return NotFound();
            return Ok(status);
        }
    }
}