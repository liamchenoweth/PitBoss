using System;
using System.Threading;

namespace PitBoss
{
    public interface IOperationHealthManager
    {
        bool Available {get;}
        void RegisterRequest(OperationRequest request);
        OperationRequestStatus GetOperationStatus(OperationRequest request);
        OperationRequestStatus GetOperationStatus(string request);
        void SetActiveOperation(OperationRequest request);
        void FailActiveOperation(OperationRequest request, Exception e);
        void FinishActiveOperation(OperationRequest request);
        OperationRequestStatus GetCurrentActiveOperationStatus();
        OperationStatus GetContainerStatus();
        CancellationToken GetCancellationToken(OperationRequest request);
        void CancelRequest(OperationRequest request);
        void CancelActiveRequests();
        void SetContainerShutdown();
    }
}