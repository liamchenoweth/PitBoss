using System;
using System.Collections.Generic;

namespace PitBoss {
    public class PipelineStep {
        internal PipelineStep() 
        {
            Id = Guid.NewGuid().ToString();
            NextSteps = new List<string>();
        }
        public PipelineStep(string script_name)
        {
            Name = script_name;
            Input = null;
            Output = null;
            Id = Guid.NewGuid().ToString();
            NextSteps = new List<string>();
        }
        public List<string> NextSteps {get; protected set;}
        public string Id {get; protected set;}
        public string Name {get; protected set;}
        public Type Input {get; protected set;}
        public Type Output {get; protected set;}
    }

    public class PipelineStep<InArg, OutArg> : PipelineStep {

        public PipelineStep(string script_name) {
            Name = script_name;
            Input = typeof(InArg);
            Output = typeof(OutArg);
        }
    }

    public class PipelineStep<InArg> : PipelineStep {
        public PipelineStep(string script_name)
        {
            Name = script_name;
            Input = typeof(InArg);
            Output = null;
        }
    }

    public class NullInputPipelineStep<OutArg> : PipelineStep {
        public NullInputPipelineStep(string script_name)
        {
            Name = script_name;
            Output = typeof(OutArg);
            Input = null;
        }
    }

    public class DistributedPipelineStep<InArg, OutArg> : PipelineStep<InArg, OutArg> where InArg : IEnumerable<InArg> {
        public DistributedPipelineStep(string script_name) : base(script_name) {}
    }
}