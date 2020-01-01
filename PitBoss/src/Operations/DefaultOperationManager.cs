using System;

namespace PitBoss {
    public class DefaultOperationManager : IOperationManager
    {
        private Delegate _operation;

        public void ProcessRequest(OperationRequest request)
        {
            
        }

        public OperationStatus GetStatus()
        {
            return new OperationStatus();
        }

        public OperationStatus GetStatus(OperationRequest request)
        {
            return new OperationStatus();
        }

        public Delegate CompileOperation(string location)
        {
            return new Func<int>(() => 0);
        }
    }
}