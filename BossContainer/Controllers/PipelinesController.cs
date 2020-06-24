using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using PitBoss.Utils;
using PitBoss.Extensions;

namespace PitBoss
{
    public class PipelinesController : Controller
    {
        private IPipelineManager _pipelineManager;
        private IPipelineRequestManager _requestManager;
        private IHttpClientFactory _clientFactory;
        private IConfiguration _configuration;
        public PipelinesController(
            IPipelineManager pipelineManager, 
            IPipelineRequestManager requestManager,
            IHttpClientFactory clientFactory,
            IConfiguration configuration)
        {
            _pipelineManager = pipelineManager;
            _requestManager = requestManager;
            _clientFactory = clientFactory;
            _configuration = configuration;
        }

        [HttpGet("api/pipelines")]
        public ActionResult GetPipelines()
        {
            return Ok(_pipelineManager.Pipelines.Select(x => new { x.Description, x.Version }));
        }

        [HttpGet("api/pipelines/{name}")]
        public ActionResult GetPipeline(string name)
        {
            return Ok(_pipelineManager.GetPipeline(name));
        }

        [HttpGet("api/pipelines/{name}/{version}")]
        public ActionResult GetPipelineByVersion(string name, string version)
        {
            return Ok(_pipelineManager.GetPipelineVersion(version));
        }

        [HttpGet("api/pipelines/{name}/{version}/requests")]
        public ActionResult PipelineRequestsForVersion(string name, string version)
        {
            return Ok(_requestManager.RequestsForPipelineVersion(version));
        }

        [HttpGet("api/pipelines/{name}/requests")]
        public ActionResult PipelineRequests(string name)
        {
            return Ok(_requestManager.RequestsForPipeline(name));
        }

        [HttpGet("api/pipelines/{name}/health")]
        public async Task<ActionResult> PipelineHealth(string name)
        {
            var pipeline = _pipelineManager.GetPipeline(name);
            if(pipeline == null || pipeline == default(Pipeline)) return BadRequest($"Pipeline {name} not found");
            var stepHealth = new List<PipelineStepStatus>();
            foreach(var step in pipeline.Steps)
            {
                stepHealth.Add(
                    await (
                        await _clientFactory
                        .CreateClient()
                        .GetAsync($"{_configuration["ContainerService:Scheme"]}://{_configuration["ContainerService:Uri"]}:{_configuration["ContainerService:Port"]}/operations/{step.Name}/health"))
                        .Content
                        .DeserialiseAsync<PipelineStepStatus>()
                    );
            }
            return Ok(new PipelineStatus{
                PipelineName = pipeline.Name,
                Steps = stepHealth,
                Health = stepHealth
                    .Where(x => x.Health == Health.Unhealthy || x.Health == Health.Warning)
                    .Count() > 0 
                    ? Health.Warning : Health.Healthy
            });
        }


    }
}