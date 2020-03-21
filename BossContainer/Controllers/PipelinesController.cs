using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using PitBoss.Utils;

namespace PitBoss
{
    public class PipelinesController : Controller
    {
        private IPipelineManager _pipelineManager;
        private IPipelineRequestManager _requestManager;
        private IContainerManager _containerManager;
        public PipelinesController(
            IPipelineManager pipelineManager, 
            IPipelineRequestManager requestManager,
            IContainerManager containerManager)
        {
            _containerManager = containerManager;
            _pipelineManager = pipelineManager;
            _requestManager = requestManager;
        }

        [HttpGet("pipelines")]
        public ActionResult GetPipelines()
        {
            return Ok(_pipelineManager.Pipelines.Select(x => x.Description));
        }

        [HttpGet("pipelines/{name}")]
        public ActionResult GetPipeline(string name)
        {
            return Ok(_pipelineManager.GetPipeline(name));
        }

        [HttpGet("pipelines/{name}/requests")]
        public ActionResult PipelineRequests(string name)
        {
            return Ok(_requestManager.RequestsForPipeline(name));
        }

        [HttpGet("pipelines/{name}/health")]
        public async Task<ActionResult> PipelineHealth(string name)
        {
            var pipeline = _pipelineManager.GetPipeline(name);
            if(pipeline == null || pipeline == default(Pipeline)) return BadRequest($"Pipeline {name} not found");
            var stepHealth = new List<PipelineStepStatus>();
            foreach(var step in pipeline.Steps)
            {
                var group = await _containerManager.GetContainersByStepAsync(step);
                var statuses = await group.GetStatusesAsync();
                if(statuses.Where(x => !x.Healthy).Count() > 0)
                {
                    if(statuses.Where(x => !x.Healthy).Count() == statuses.Count())
                    {
                        stepHealth.Add(new PipelineStepStatus{
                            PipelineStepName = step.Name,
                            Health = Health.Unhealthy
                        });
                        continue;
                    }
                    stepHealth.Add(new PipelineStepStatus{
                        PipelineStepName = step.Name,
                        Health = Health.Warning
                    });
                    continue;
                }
                stepHealth.Add(new PipelineStepStatus{
                    PipelineStepName = step.Name,
                    Health = Health.Healthy
                });
                continue;
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