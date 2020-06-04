using System.Threading;
using System.Threading.Tasks;

namespace PitBoss
{
    public interface IDistributedStepService
    {
        Task MonitorDistributedRequests(CancellationToken cancelationToken);
    }
}