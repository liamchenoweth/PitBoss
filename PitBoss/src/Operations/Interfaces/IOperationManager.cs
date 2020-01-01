using System;

namespace PitBoss {
    public interface IOperationManager
    {
        void ProcessRequest(OperationRequest request);

        OperationStatus GetStatus();

        OperationStatus GetStatus(OperationRequest request);
        
        Delegate CompileOperation(string location);
    }
}