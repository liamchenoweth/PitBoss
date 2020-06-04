using System.Threading;
using System.Threading.Tasks;

namespace PitBoss
{
    public interface IContainerService
    {
        Task BalanceContainers(CancellationToken cancelationToken);
    }
}