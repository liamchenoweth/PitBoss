using System;
using System.Threading.Tasks;
using System.IO;

namespace PitBoss {
    public interface IOperationManager
    {
        bool Ready {get;}
        Type InputType {get;}
        Type OutputType {get;}
        void QueueRequest(OperationRequest request);
        OperationRequest GetNextRequest();
        Task<object> ProcessRequest(OperationRequest request);
        Delegate CompileOperation(string location);
        Task<Delegate> CompileOperationAsync(string location);
        Task<OperationRequest> DeserialiseRequestAsync(Stream requestJson);
        OperationRequest DeserialiseRequest(Stream requestJson);
        OperationRequest DeserialiseRequest(string requestJson);
        void FinishRequest(OperationRequest request, object output);
        Task FinishRequestAsync(OperationRequest request, object output);
    }
}