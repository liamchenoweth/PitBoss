using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using PitBoss.Utils;

namespace PitBoss
{
    public class ConatinerController : Controller
    {
        private IContainerManager _containerManager;

        public ConatinerController(IContainerManager containerManager) {
            _containerManager = containerManager;
        }

        [HttpGet("operations")]
        public async Task<ActionResult> GetOperations()
        {
            var operationGroups = await Task.WhenAll((await _containerManager.GetContainersAsync()).Select(async x => await x.GetDescriptionAsync()));
            return Ok(operationGroups);
        }

        [HttpGet("operations/{name}/health")]
        public async Task<ActionResult> GetContainerHealth(string name)
        {
            var group = await _containerManager.GetContainersByStepAsync(new PipelineStep(name));
            var statuses = await group.GetStatusesAsync();
            if(statuses.Where(x => !x.Healthy).Count() > 0)
            {
                if(statuses.Where(x => !x.Healthy).Count() == statuses.Count())
                {
                    return Ok(new PipelineStepStatus{
                        PipelineStepName = name,
                        Health = Health.Unhealthy
                    });
                }
                return Ok(new PipelineStepStatus{
                    PipelineStepName = name,
                    Health = Health.Warning
                });
            }
            return Ok(new PipelineStepStatus{
                PipelineStepName = name,
                Health = Health.Healthy
            });
        }

        [HttpGet("operations/{name}/containers")]
        public async Task<ActionResult> GetContainers(string name)
        {
            var group = await _containerManager.GetContainersByStepAsync(new PipelineStep(name));
            var descriptions = await Task.WhenAll((await group.GetContainersAsync()).Select(async x => await x.GetDescriptionAsync()));
            return Ok(descriptions);
        }

        [HttpDelete("operations/{name}/containers/{id}")]
        public async Task<ActionResult> DeleteContainer(string name, string id)
        {
            var group = await _containerManager.GetContainersByStepAsync(new PipelineStep(name));
            var container = group.SingleOrDefault(x => x.Name == id);
            if(container == default) return NotFound();
            await group.RemoveContainerAsync(container);
            return Ok();
        }

        [HttpGet("operations/containers")]
        public async Task<ActionResult> GetAllContainers(string name, string id)
        {
            var groups = await _containerManager.GetContainersAsync();
            return Ok(groups);
        }
    }
    
}