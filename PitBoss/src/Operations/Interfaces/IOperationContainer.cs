using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace PitBoss
{
    
    public interface IOperationContainer
    {
        string Name { get; }
        string Url { get; }
        string Operation { get; }
        string Id { get; }
        //Pipeline Pipeline { get; }

        void SendOperation(string location);
        Task SendOperationAsync(string location);
        void SendRequest(OperationRequest request);
        Task SendRequestAsync(OperationRequest request);
        bool SendShutdown();
        Task<bool> SendShutdownAsync();
        void ForceShutdown();
        Task ForceShutdownAsync();
        OperationStatus GetStatus();
        Task<OperationStatus> GetStatusAsync();
        OperationStatus GetStatus(OperationRequest request);
        Task<OperationStatus> GetStatusAsync(OperationRequest request);
    }
    
}