using System.Collections.Generic;

namespace PitBoss
{
    public interface IDistributedOperation<TIn, TOut> : IOperation<TIn, TOut>
    {

    }
    public class DistributedStep<InArg, OutArg> : PipelineStep<IEnumerable<InArg>, IEnumerable<OutArg>> 
    {
        public DistributedStep(string script_name) : base(script_name) {}
    }
}