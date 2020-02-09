using System;
using System.Threading.Tasks;
using System.IO;

namespace PitBoss {
    public interface IOperationManager
    {
        object ProcessRequest(OperationRequest request);
        OperationStatus GetStatus();
        OperationStatus GetStatus(OperationRequest request);
        Delegate CompileOperation(string location);
        Task<OperationRequest> DeserialiseRequestAsync(Stream requestJson);
        OperationRequest DeserialiseRequest(Stream requestJson);
        OperationRequest DeserialiseRequest(string requestJson);
        void FinishRequest(OperationRequest request, object output);
        Task FinishRequestAsync(OperationRequest request, object output);
    }
}