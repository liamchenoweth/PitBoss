using System.Collections.Generic;
using System.Threading.Tasks;

namespace PitBoss
{
    public interface IOperationGroup : IEnumerable<IOperationContainer>
    {
        PipelineStep PipelineStep {get;}
        int CurrentSize();
        Task<int> CurrentSizeAsync();
        int TargetSize {get;}
        void SendRequest(OperationRequest request);
        Task SendRequestAsync(OperationRequest request);
        IEnumerable<OperationStatus> GetStatuses();
        Task<IEnumerable<OperationStatus>> GetStatusesAsync();
        OperationGroupStatus GetGroupHealth();
        Task<OperationGroupStatus> GetGroupHealthAsync();
        void AddContainer(IOperationContainer container);
        void RemoveContainer(IOperationContainer container);
        Task AddContainerAsync(IOperationContainer container);
        Task RemoveContainerAsync(IOperationContainer container);
        IEnumerable<IOperationContainer> GetHealthyContainers();
        Task<IEnumerable<IOperationContainer>> GetHealthyContainersAsync();
        IEnumerable<IOperationContainer> GetUnhealthyContainers();
        Task<IEnumerable<IOperationContainer>> GetUnhealthyContainersAsync();
        IEnumerable<IOperationContainer> GetContainers();
        Task<IEnumerable<IOperationContainer>> GetContainersAsync();
        GroupDescription GetDescription();
        Task<GroupDescription> GetDescriptionAsync();
    }
}