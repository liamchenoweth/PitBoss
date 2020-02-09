using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace PitBoss
{
    public class DefaultOperationGroup : IOperationGroup
    {
        public PipelineStep PipelineStep {get; private set;}
        private List<IOperationContainer> _containers;

        public void SendRequest(OperationRequest request)
        {
            SendRequestAsync(request).RunSynchronously();
        }

        public async Task SendRequestAsync(OperationRequest request)
        {
            foreach(var container in _containers)
            {
                var status = await container.GetStatusAsync();
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
                statuses.Add(await container.GetStatusAsync());
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
                health.Add(await container.GetStatusAsync());
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
            _containers.Add(container);
        }

        public void RemoveContainer(IOperationContainer container)
        {
            _containers.Remove(container);
        }
    }
}