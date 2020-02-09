using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace PitBoss
{
    public static class IContainerBalancerExtensions
    {
        public static IServiceCollection UseContainerBalancer<T>(this IServiceCollection collection) where T : class, IContainerBalancer
        {
            collection.AddSingleton<IContainerBalancer, T>();
            return collection;
        }
    }
    public interface IContainerBalancer
    {
        void BalanceGroup(IOperationGroup group);
        Task BalanceGroupAsync(IOperationGroup group);
    }
}