using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;

namespace PitBoss {
    public class DefaultContainerManager : IContainerManager {
        private Dictionary<string, IOperationContainer> _containers;
        private IHttpClientFactory _clientFactory;

        public DefaultContainerManager(IHttpClientFactory clientFactory) 
        {
            _containers = new Dictionary<string, IOperationContainer>();
            _clientFactory = clientFactory;
        }

        public IEnumerable<IOperationContainer> GetContainers()
        {
            return _containers.Values;
        }

        public async Task<IEnumerable<IOperationContainer>> GetContainersAsync()
        {
            return _containers.Values;
        }

        public IOperationContainer CreateContainer(PipelineStep step)
        {
            var container = new DefaultOperationContainer(step, _clientFactory);
            _containers.Add(container.Name, container);
            return container;    
        }

        public async Task<IOperationContainer> CreateContainerAsync(PipelineStep step)
        {
            var container = new DefaultOperationContainer(step, _clientFactory);
            _containers.Add(container.Name, container);
            return container; 
        }

        public void DestroyContainer(IOperationContainer container)
        {
            DestroyContainerAsync(container).RunSynchronously();
        }

        public async Task DestroyContainerAsync(IOperationContainer container)
        {
            if(!_containers.ContainsKey(container.Name)) throw new KeyNotFoundException($"{container.Name} is not registered with this manager");
            if(!(await container.SendShutdownAsync()))
            {
                await container.ForceShutdownAsync();
                _containers.Remove(container.Name);
            }
        }

        public IEnumerable<IOperationContainer> GetContainersByStep(PipelineStep step)
        {
            var task = GetContainersByStepAsync(step);
            task.RunSynchronously();
            return task.Result;
        }

        public async Task<IEnumerable<IOperationContainer>> GetContainersByStepAsync(PipelineStep step)
        {
            return _containers.Values.Where(x => x.Operation == step.Name);
        }

        public IEnumerable<IOperationContainer> GetContainersByName(string name)
        {
            var task = GetContainersByNameAsync(name);
            task.RunSynchronously();
            return task.Result;
        }

        public async Task<IEnumerable<IOperationContainer>> GetContainersByNameAsync(string name)
        {
            return _containers.Values.Where(x => x.Name == name);
        }

        public IEnumerable<IOperationContainer> GetContainersByUrl(string url)
        {
            var task = GetContainersByUrlAsync(url);
            task.RunSynchronously();
            return task.Result;
        }

        public async Task<IEnumerable<IOperationContainer>> GetContainersByUrlAsync(string url)
        {
            return _containers.Values.Where(x => x.Url == url);
        }

        // We don't need to discover containers here because this is a singleton
        public void DiscoverContainers() {}

        public async Task DiscoverContainersAsync() {}
    }
}