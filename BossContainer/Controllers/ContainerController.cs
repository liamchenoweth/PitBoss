using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PitBoss.Utils;
using PitBoss.Extensions;

namespace PitBoss
{
    public class ContainerController : Controller
    {
        private IHttpClientFactory _clientFactory;
        private IConfiguration _configuration;

        public ContainerController(
            IHttpClientFactory clientFactory,
            IConfiguration configuration) {
            _clientFactory = clientFactory;
            _configuration = configuration;
        }

        [HttpGet("api/operations")]
        public async Task<ActionResult> GetOperations()
        {
            var descriptions = await (await _clientFactory
                        .CreateClient()
                        .GetAsync($"{_configuration["ContainerService:Scheme"]}://{_configuration["ContainerService:Uri"]}:{_configuration["ContainerService:Port"]}/operations"))
                        .Content
                        .DeserialiseAsync<List<GroupDescription>>();
            return Ok(descriptions);
        }

        [HttpGet("api/operations/{name}/health")]
        public async Task<ActionResult> GetContainerHealth(string name)
        {
            var status = await (await _clientFactory
                        .CreateClient()
                        .GetAsync($"{_configuration["ContainerService:Scheme"]}://{_configuration["ContainerService:Uri"]}:{_configuration["ContainerService:Port"]}/operations/{name}/health"))
                        .Content
                        .DeserialiseAsync<PipelineStepStatus>();
            return Ok(status);
        }

        [HttpGet("api/operations/{name}/containers")]
        public async Task<ActionResult> GetContainers(string name)
        {
            var descriptions = await (await _clientFactory
                        .CreateClient()
                        .GetAsync($"{_configuration["ContainerService:Scheme"]}://{_configuration["ContainerService:Uri"]}:{_configuration["ContainerService:Port"]}/operations/{name}/containers"))
                        .Content
                        .DeserialiseAsync<List<OperationDescription>>();
            return Ok(descriptions);
        }

        [HttpDelete("api/operations/{name}/containers/{id}")]
        public async Task<ActionResult> DeleteContainer(string name, string id)
        {
            var op = await _clientFactory
                        .CreateClient()
                        .DeleteAsync($"{_configuration["ContainerService:Scheme"]}://{_configuration["ContainerService:Uri"]}:{_configuration["ContainerService:Port"]}/operations/{name}/containers/{id}");
            return StatusCode((int)op.StatusCode, op.Content);
        }

        [HttpGet("api/operations/containers")]
        public async Task<ActionResult> GetAllContainers(string name, string id)
        {
            var groups = await (await _clientFactory
                        .CreateClient()
                        .GetAsync($"{_configuration["ContainerService:Scheme"]}://{_configuration["ContainerService:Uri"]}:{_configuration["ContainerService:Port"]}/operations/containers"))
                        .Content
                        .DeserialiseAsync<List<DefaultOperationGroup>>();
            return Ok(groups);
        }
    }
    
}