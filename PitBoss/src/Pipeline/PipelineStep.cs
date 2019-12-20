using System;
using System.Collections.Generic;

namespace PitBoss {
    public abstract class PipelineStep {
        protected string _name;
    }

    public class PipelineStep<InArg, OutArg> : PipelineStep {

        public PipelineStep(string script_name) {
            _name = script_name;
        }
    }

    public class DistributedPipelineStep<InArg, OutArg> : PipelineStep<InArg, OutArg> where InArg : IEnumerable<InArg> {
        public DistributedPipelineStep(string script_name) : base(script_name) {}
    }
}