#if LOCAL_DEV
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;

namespace PitBoss {
    public class DefaultContainerManager : IContainerManager {
        private Dictionary<string, IOperationGroup> _containers;
        private IHttpClientFactory _clientFactory;

        public DefaultContainerManager(IHttpClientFactory clientFactory) 
        {
            _containers = new Dictionary<string, IOperationGroup>();
            _clientFactory = clientFactory;
            Ready = true;
        }

        public bool Ready {get; private set;} = false;

        public IEnumerable<IOperationGroup> GetContainers()
        {
            return _containers.Values;
        }

        public async Task<IEnumerable<IOperationGroup>> GetContainersAsync()
        {
            return _containers.Values;
        }

        public IOperationGroup GetContainersByStep(PipelineStep step)
        {
            var task = GetContainersByStepAsync(step);
            task.Wait();
            return task.Result;
        }

        public async Task<IOperationGroup> GetContainersByStepAsync(PipelineStep step)
        {
            if(_containers.TryGetValue(step.Name, out var group)) return group;
            throw new Exception($"Container group does not exist for {step.Name}");
        }

        public void SetGroupsStale()
        {
            foreach(var group in _containers)
            {
                group.Value.Stale = true;
            }
        }

        public void RegisterGroup(IOperationGroup group)
        {
            RegisterGroupAsync(group).Wait();
        }

        public async Task RegisterGroupAsync(IOperationGroup group)
        {
            if(_containers.ContainsKey(group.PipelineStep.Name))
            {
                _containers[group.PipelineStep.Name].Stale = false;
                return;
            }
            _containers.Add(group.PipelineStep.Name, group);
        }

        public void RemoveGroup(IOperationGroup group)
        {
            _containers.Remove(group.PipelineStep.Name);
        }

        // We don't need to discover containers here because this is a singleton
        public void DiscoverContainers() {}

        public async Task DiscoverContainersAsync() {}

        public void RunShutdown()
        {
            RunShutdownAsync().GetAwaiter().GetResult();
        }

        public async Task RunShutdownAsync()
        {
            foreach(var pair in _containers)
            {
                var group = pair.Value;
                await group.ShutdownGroupAsync();
            }
            _containers.Clear();
        }
    }
}
#endif