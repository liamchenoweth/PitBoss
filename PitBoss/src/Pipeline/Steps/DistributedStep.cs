using System.Collections.Generic;

namespace PitBoss
{
    public class DistributedStep<InArg, OutArg> : PipelineStep<InArg, OutArg> where InArg : IEnumerable<InArg> {
        public DistributedStep(string script_name) : base(script_name) {}
    }
}