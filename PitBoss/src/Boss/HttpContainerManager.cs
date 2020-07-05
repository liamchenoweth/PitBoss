using System;
using System.Linq;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using PitBoss.Extensions;

namespace PitBoss
{
    public class HttpContainerManager<T> : IContainerManager where T : IOperationGroup, new()
    {
        private IHttpClientFactory _clientFactory;
        private IConfiguration _configuration;
        private IPipelineManager _pipelineManager;
        private ILogger _logger;

        public HttpContainerManager(
            IPipelineManager pipelineManager,
            IHttpClientFactory clientFactory,
            IConfiguration configuration,
            ILogger<HttpContainerManager<T>> logger) 
        {
            _pipelineManager = pipelineManager;
            _configuration = configuration;
            _clientFactory = clientFactory;
            _logger = logger;
            Ready = true;
        }

        public bool Ready {get; private set;} = false;

        public IEnumerable<IOperationGroup> GetContainers()
        {
            var task = GetContainersAsync();
            task.Wait();
            return task.Result;
        }

        public void SetGroupsStale()
        {
            throw new NotImplementedException("Can't set group stale with the HttpContainerManager");
        }

        public async Task<IEnumerable<IOperationGroup>> GetContainersAsync()
        {
            return (await (await (_clientFactory
                .CreateClient()
                .GetAsync($"{_configuration["ContainerService:Scheme"]}://{_configuration["ContainerService:Host"]}:{_configuration["ContainerService:Port"]}/operations/containers")))
                .Content.DeserialiseAsync<List<T>>()).Cast<IOperationGroup>();
        }

        public IOperationGroup GetContainersByStep(PipelineStep step)
        {
            var task = GetContainersByStepAsync(step);
            task.Wait();
            return task.Result;
        }

        public async Task<IOperationGroup> GetContainersByStepAsync(PipelineStep step)
        {
            var descriptions = (await (await (_clientFactory
                .CreateClient()
                .GetAsync($"{_configuration["ContainerService:Scheme"]}://{_configuration["ContainerService:Uri"]}:{_configuration["ContainerService:Port"]}/operations/{step.Name}/containers")))
                .Content.DeserialiseAsync<List<OperationDescription>>());

            var containers = descriptions.Select(x => new HttpContainer(step, _clientFactory, _logger, _configuration, x));

            var ret = new T();
            ret.InflateFromDescription(_pipelineManager, new GroupDescription{
                Name = step.Name,
                TargetSize = 0
            });

            await ret.SetContainersAsync(containers);

            return ret;
        }

        public void RemoveGroup(IOperationGroup group)
        {
            throw new NotImplementedException("Can't remove group with the HttpContainerManager");
        }

        public void RegisterGroup(IOperationGroup group)
        {
            RegisterGroupAsync(group).Wait();
        }

        public async Task RegisterGroupAsync(IOperationGroup group)
        {
            throw new NotImplementedException("Can't register a new group with the HttpContainerManager");
        }

        // We don't need to discover containers here because this is a singleton
        public void DiscoverContainers() {}

        public async Task DiscoverContainersAsync() {}

        public void RunShutdown()
        {
            throw new NotImplementedException("HttpContainerManager cannot shutdown containers");
        }

        public async Task RunShutdownAsync()
        {
            throw new NotImplementedException("HttpContainerManager cannot shutdown containers");
        }
    }
}