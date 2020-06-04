using System.Threading;
using System.Threading.Tasks;

namespace PitBoss
{
    public interface IOperationGroupService
    {
        Task MonitorRequests(CancellationToken cancelationToken);
    }
}