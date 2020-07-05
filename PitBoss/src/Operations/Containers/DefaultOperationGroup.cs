using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

using PitBoss.Utils;
namespace PitBoss
{
    public class DefaultOperationGroup : IOperationGroup
    {
        public PipelineStep PipelineStep {get; private set;}
        public int TargetSize {get; private set;}
        public bool Stale {get; set;} = false;
        private List<IOperationContainer> _containers;

        public DefaultOperationGroup() { _containers = new List<IOperationContainer>(); }

        public DefaultOperationGroup(PipelineStep step)
        {
            PipelineStep = step;
            TargetSize = step.TargetCount;
            _containers = new List<IOperationContainer>();
        }

        public void InflateFromDescription(IPipelineManager pipelineManager, GroupDescription description)
        {
            PipelineStep = pipelineManager.Pipelines.SelectMany(x => x.Steps).Where(x => x.Name == description.Name).First();
            TargetSize = description.TargetSize;
        }

        public void SendRequest(OperationRequest request)
        {
            SendRequestAsync(request).Wait();
        }

        public int CurrentSize()
        {
            var task = CurrentSizeAsync();
            task.Wait();
            return task.Result;
        }

        public async Task<int> CurrentSizeAsync()
        {
            var count = 0;
            var toRemove = new List<IOperationContainer>();
            foreach(var container in _containers)
            {
                var status = await container.GetContainerStatusAsync();
                if(status.Healthy){
                    count++;
                    continue;
                }
                toRemove.Add(container);
            }
            toRemove.ForEach(x => _containers.Remove(x));
            return count;
        }

        public IEnumerable<IOperationContainer> GetContainers()
        {
            var task = GetContainersAsync();
            task.Wait();
            return task.Result;
        }

        public async Task<IEnumerable<IOperationContainer>> GetContainersAsync()
        {
            return _containers;
        }

        public void SetContainers(IEnumerable<IOperationContainer> containers)
        {
            SetContainersAsync(containers).Wait();
        }

        public async Task SetContainersAsync(IEnumerable<IOperationContainer> containers)
        {
            _containers = containers.ToList();
        }

        public IEnumerable<IOperationContainer> GetHealthyContainers()
        {
            var task = GetHealthyContainersAsync();
            task.Wait();
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
            task.Wait();
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
            task.Wait();
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
            task.Wait();
            return task.Result;
        }

        public async Task<OperationGroupStatus> GetGroupHealthAsync()
        {
            var health = new List<OperationStatus>();
            foreach(var container in _containers)
            {
                health.Add(await container.GetContainerStatusAsync());
            }
            var groupHealth = Health.Unknown;
            var healthy = health.Where(x => x.Healthy).Count();
            var unhealthy = (_containers.Count - healthy);
            var ready = health.Where(x => x.ContainerStatus == ContainerStatus.Ready).Count();
            var processing = health.Where(x => x.ContainerStatus == ContainerStatus.Processing).Count();
            if(unhealthy == 0) groupHealth = Health.Healthy;
            else if(healthy > 0) groupHealth = Health.Warning;
            else groupHealth = Health.Unhealthy;
            return new OperationGroupStatus
            {
                Containers = _containers.Count,
                HealthyContainers = healthy,
                UnhealthyContainers = unhealthy,
                ReadyContainers = ready,
                ProcessingContainers = processing,
                GroupHealth = groupHealth
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
            AddContainerAsync(container).Wait();
        }

        public async Task AddContainerAsync(IOperationContainer container)
        {
            await container.SendOperationAsync(PipelineStep.Name);
            _containers.Add(container);
        }

        public void RemoveContainer(IOperationContainer container)
        {
            RemoveContainerAsync(container).Wait();
        }

        public async Task RemoveContainerAsync(IOperationContainer container)
        {
            await container.SendShutdownAsync();
            _containers.Remove(container);
        }

        public GroupDescription GetDescription()
        {
            var task = GetDescriptionAsync();
            task.Wait();
            return task.Result;
        }

        public async Task<GroupDescription> GetDescriptionAsync()
        {
            return new GroupDescription {
                Name = PipelineStep.Name,
                Script = PipelineStep.Name,
                TargetSize = TargetSize,
                CurrentSize = await CurrentSizeAsync(),
                Status = await GetGroupHealthAsync()
            };
        }

        public void ShutdownGroup()
        {
            ShutdownGroupAsync().GetAwaiter().GetResult();
        }

        public async Task ShutdownGroupAsync()
        {
            foreach(var container in _containers)
            {
                await container.SendShutdownAsync();
            }
            _containers.RemoveAll(x => true);
        }
    }
}