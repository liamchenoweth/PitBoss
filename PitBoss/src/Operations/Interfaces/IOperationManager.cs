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
        void CompileOperation(string location);
        Task CompileOperationAsync(string location);
        Task<OperationRequest> DeserialiseRequestAsync(Stream requestJson);
        OperationRequest DeserialiseRequest(Stream requestJson);
        OperationRequest DeserialiseRequest(string requestJson);
        void FinishRequest(OperationRequest request, object output, bool failed);
        Task FinishRequestAsync(OperationRequest request, object output, bool failed);
    }
}