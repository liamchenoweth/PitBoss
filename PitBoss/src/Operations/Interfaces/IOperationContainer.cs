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
        OperationStatus GetContainerStatus();
        Task<OperationStatus> GetContainerStatusAsync();
        OperationRequestStatus GetRequestStatus(OperationRequest request);
        Task<OperationRequestStatus> GetRequestStatusAsync(OperationRequest request);
        OperationDescription GetDescription();
        Task<OperationDescription> GetDescriptionAsync();
    }
    
}