using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace PitBoss
{
    public static class IContainerManagerExtensions
    {
        public static IServiceCollection UseContainerManager<T>(this IServiceCollection collection) where T : class, IContainerManager
        {
            collection.AddSingleton<IContainerManager, T>();
            return collection;
        }
    }
    public interface IContainerManager
    {
        bool Ready {get;}
        IEnumerable<IOperationGroup> GetContainers();
        Task<IEnumerable<IOperationGroup>> GetContainersAsync();
        IOperationContainer CreateContainer(PipelineStep step);
        Task<IOperationContainer> CreateContainerAsync(PipelineStep step);
        void DestroyContainer(IOperationContainer container);
        Task DestroyContainerAsync(IOperationContainer container);
        IOperationGroup GetContainersByStep(PipelineStep step);
        Task<IOperationGroup> GetContainersByStepAsync(PipelineStep step);
        void DiscoverContainers();
        Task DiscoverContainersAsync();
    }
    
}