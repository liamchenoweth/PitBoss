using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace PitBoss
{
    public class ConatinersController : Controller
    {
        private IPipelineManager _pipelineManager;
        private IOperationRequestManager _requestManager;
        private IPipelineRequestManager _pipelineRequestManager;
        private IContainerManager _containerManager;

        public ConatinersController(
            IPipelineManager pipelineManager, 
            IOperationRequestManager requestManager,
            IPipelineRequestManager pipelineRequestManager,
            IContainerManager containerManager) {
            _pipelineManager = pipelineManager;
            _requestManager = requestManager;
            _pipelineRequestManager = pipelineRequestManager;
            _containerManager = containerManager;
        }

        [HttpGet("operations")]
        public async Task<ActionResult> GetOperations()
        {
            var operationGroups = await Task.WhenAll((await _containerManager.GetContainersAsync()).Select(async x => await x.GetDescriptionAsync()));
            return Ok(operationGroups);
        }

        [HttpGet("operations/{name}/containers")]
        public async Task<ActionResult> GetContainers(string name)
        {
            var group = await _containerManager.GetContainersByStepAsync(new PipelineStep(name));
            var descriptions = await Task.WhenAll((await group.GetContainersAsync()).Select(async x => await x.GetDescriptionAsync()));
            return Ok(descriptions);
        }

        // Create operation/result
        [HttpPost("operation/result")]
        public ActionResult PostResult([FromBody]JObject result)
        {
            var prelimResult = result.ToObject<OperationResponse>();
            if(prelimResult == null) return BadRequest();
            var pipeline = _pipelineManager.GetPipeline(prelimResult.PipelineName);
            var step = pipeline.Steps[prelimResult.PipelineStepId];
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