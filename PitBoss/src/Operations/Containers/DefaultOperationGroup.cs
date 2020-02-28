using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace PitBoss
{
    public class DefaultOperationGroup : IOperationGroup
    {
        public PipelineStep PipelineStep {get; private set;}
        public int TargetSize {get; private set;}
        private List<IOperationContainer> _containers;

        public DefaultOperationGroup(PipelineStep step)
        {
            PipelineStep = step;
            TargetSize = 1; // TODO: inform this from the step / config
            _containers = new List<IOperationContainer>();
        }

        public void SendRequest(OperationRequest request)
        {
            SendRequestAsync(request).RunSynchronously();
        }

        public int CurrentSize()
        {
            var task = CurrentSizeAsync();
            task.RunSynchronously();
            return task.Result;
        }

        public async Task<int> CurrentSizeAsync()
        {
            return _containers.Count;
        }

        public IEnumerable<IOperationContainer> GetHealthyContainers()
        {
            var task = GetHealthyContainersAsync();
            task.RunSynchronously();
            return task.Result;
        }

        public async Task<IEnumerable<IOperationContainer>> GetHealthyContainersAsync()
        {
            var healthy = new List<IOperationContainer>();
            foreach(var container in _containers)
            {
                var status = await container.GetContainerStatusAsync();
                if(status.Healthy) healthy.Add(container);
            }
            return healthy;
        }

        public IEnumerable<IOperationContainer> GetUnhealthyContainers()
        {
            var task = GetUnhealthyContainersAsync();
            task.RunSynchronously();
            return task.Result;
        }

        public async Task<IEnumerable<IOperationContainer>> GetUnhealthyContainersAsync()
        {
            var unhealthy = new List<IOperationContainer>();
            foreach(var container in _containers)
            {
                var status = await container.GetContainerStatusAsync();
                if(!status.Healthy) unhealthy.Add(container);
            }
            return unhealthy;
        }

        public async Task SendRequestAsync(OperationRequest request)
        {
            foreach(var container in _containers)
            {
                var status = await container.GetContainerStatusAsync();
                if(status.ContainerStatus == ContainerStatus.Ready)
                {
                    await container.SendRequestAsync(request);
                    return;
                }
            }
            throw new ContainerUnavailableException(this);
        }

        public IEnumerable<OperationStatus> GetStatuses()
        {
            var task = GetStatusesAsync();
            task.RunSynchronously();
            return task.Result;
        }

        public async Task<IEnumerable<OperationStatus>> GetStatusesAsync()
        {
            var statuses = new List<OperationStatus>();
            foreach(var container in _containers)
            {
                statuses.Add(await container.GetContainerStatusAsync());
            }
            return statuses.Where(x => x != null);
        }

        public OperationGroupStatus GetGroupHealth()
        {
            var task = GetGroupHealthAsync();
            task.RunSynchronously();
            return task.Result;
        }

        public async Task<OperationGroupStatus> GetGroupHealthAsync()
        {
            var health = new List<OperationStatus>();
            foreach(var container in _containers)
            {
                health.Add(await container.GetContainerStatusAsync());
            }
            return new OperationGroupStatus
            {
                Containers = _containers.Count,
                HealthyContainers = health.Where(x => x.Healthy).Count(),
                UnhealthyContainers = (_containers.Count - health.Where(x => x.Healthy).Count()),
                ReadyContainers = health.Where(x => x.ContainerStatus == ContainerStatus.Ready).Count(),
                ProcessingContainers = health.Where(x => x.ContainerStatus == ContainerStatus.Processing).Count()
            };
        }

        IEnumerator<IOperationContainer> IEnumerable<IOperationContainer>.GetEnumerator()
        {
            return _containers.GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return _containers.GetEnumerator();
        }

        public void AddContainer(IOperationContainer container)
        {
            AddContainerAsync(container).RunSynchronously();
        }

        public async Task AddContainerAsync(IOperationContainer container)
        {
            await container.SendOperationAsync(PipelineStep.Name);
            _containers.Add(container);
        }

        public void RemoveContainer(IOperationContainer container)
        {
            RemoveContainerAsync(container).RunSynchronously();
        }

        public async Task RemoveContainerAsync(IOperationContainer container)
        {
            await container.SendShutdownAsync();
            _containers.Remove(container);
        }
    }
}