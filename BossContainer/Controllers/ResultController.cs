using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace PitBoss
{
    public class ResultController : Controller
    {
        private IPipelineManager _pipelineManager;
        private IOperationRequestManager _requestManager;
        private IPipelineRequestManager _pipelineRequestManager;

        public ResultController(
            IPipelineManager pipelineManager, 
            IOperationRequestManager requestManager,
            IPipelineRequestManager pipelineRequestManager) {
            _pipelineManager = pipelineManager;
            _requestManager = requestManager;
            _pipelineRequestManager = pipelineRequestManager;
        }

        // Create operation/result
        [HttpPost("api/operation/result")]
        public ActionResult PostResult([FromBody]JObject result)
        {
            var prelimResult = result.ToObject<OperationResponse>();
            if(prelimResult == null) return BadRequest();
            var pipeline = _pipelineManager.GetPipeline(prelimResult.PipelineName);
            var step = pipeline.Steps.Single(x => x.Id == prelimResult.PipelineStepId);
            OperationResponse fullResult = null;
            if(step.Output == null)
            {
                fullResult = prelimResult;
            }
            else
            {
                var returnType = typeof(OperationResponse<>).MakeGenericType(step.Output);
                fullResult = (OperationResponse)result.ToObject(returnType);
            }
            if(_requestManager.ProcessResponse(fullResult))
            {
                _pipelineRequestManager.FinishRequest(fullResult);
            }
            return Ok();
        }
    }
    
}