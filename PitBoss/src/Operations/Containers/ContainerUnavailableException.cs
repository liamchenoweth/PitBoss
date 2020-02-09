using System;

namespace PitBoss
{
    public class ContainerUnavailableException : Exception
    {
        public ContainerUnavailableException() {}
        public ContainerUnavailableException(string message) : base(message) {}
        public ContainerUnavailableException(string message, Exception innerException) : base(message, innerException) {}
        public ContainerUnavailableException(IOperationGroup group) : base($"No containers available for step {group.PipelineStep.Name}") {}
    }
}