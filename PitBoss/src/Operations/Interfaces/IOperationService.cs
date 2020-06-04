using System.Threading;
using System.Threading.Tasks;

namespace PitBoss
{
    public interface IOperationService
    {
        Task PollRequests(CancellationToken cancelationToken);
    }
}