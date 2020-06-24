namespace PitBoss {
    public enum RetryStrategy
    {
        Inherit,
        None,
        Linear,
        Exponential
    }

    public class PipelineDescriptor {
        public string Name {get;set;}
        public string BranchId {get;set;}
        public RetryStrategy RetryStrategy {get;set;}
    }
}