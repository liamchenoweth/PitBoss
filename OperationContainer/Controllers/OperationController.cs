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

        private void ensureLocation(string location)
        {
            var folders = location.Split(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            var current = "/";
            foreach(var folder in folders)
            {
                current = Path.Join(current, folder);
                if(Directory.Exists(current) || System.IO.File.Exists(current))
                {
                    continue;
                }
                Directory.CreateDirectory(current);
            }
        }

        [HttpPost("operation/script")]
        public async Task<ActionResult> RecieveScript(List<IFormFile> executionScript) 
        {
            var baseLocation = "OperationContainer/";
            foreach(var file in executionScript)
            {
                if(file.Length > 0)
                {
                    if(Path.IsPathRooted(file.FileName)) baseLocation = "";
                    var location = Path.GetFullPath($"{baseLocation}{file.FileName}");
                    ensureLocation(Directory.GetParent(location).FullName);
                    using(var fileStream = new FileStream(location, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                    await _operationManager.CompileOperationAsync(location);
                }
            }
            return Ok();
        }

        [HttpPost("operation/request")]
        public async Task<OperationRequestStatus> RequestOperation([FromBody]JObject jrequest) 
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