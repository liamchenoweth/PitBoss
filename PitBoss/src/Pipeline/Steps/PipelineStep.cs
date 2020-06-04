using System;
using System.Linq;
using System.Collections.Generic;

namespace PitBoss {
    public class PipelineStepOptions
    {
        public string DisplayName {get;set;}
        public int TargetCount {get;set;}
    }

    public class PipelineStep {
        internal PipelineStep() 
        {
            Id = Guid.NewGuid().ToString();
            NextSteps = new List<string>();
            IsBranch = false;
        }
        public PipelineStep(string script_name, PipelineStepOptions options = null)
        {
            Name = script_name;
            DisplayName = options?.DisplayName ?? Name;
            TargetCount = options?.TargetCount ?? 1;
            Input = null;
            Output = null;
            Id = Guid.NewGuid().ToString();
            NextSteps = new List<string>();
            IsBranch = false;
        }
        public List<string> NextSteps {get; protected set;}
        public bool IsBranch {get; protected set;}
        public string BranchEndId {get; protected set;}
        public string Id {get; protected set;}
        public string Name {get; protected set;}
        public string DisplayName {get; protected set;}
        public int TargetCount {get; protected set;}
        public Type Input {get; protected set;}
        public Type Output {get; protected set;}
        public bool IsDistributedStart {get; internal set;}
        public bool IsDistributed { get; internal set;}
        public string DistributedEndId {get; internal set;}
        public bool IsDistributedEnd { get => Id == DistributedEndId;}

        public virtual string GetNextStep(OperationResponse response)
        {
            return NextSteps.FirstOrDefault();
        }

        public virtual void AddAsNextSteps(List<string> steps)
        {
            steps.Add(this.Id);
        }

        public virtual void AddStepsToPipeline(List<PipelineStep> steps)
        {
            steps.Add(this);
        }

        internal virtual void SetStepId(string Id)
        {
            this.Id = Id;
        }
    }

    public class PipelineStep<InArg, OutArg> : PipelineStep {

        internal PipelineStep() {
            Input = typeof(InArg);
            Output = typeof(OutArg);
        }

        public PipelineStep(string script_name, PipelineStepOptions options = null) {
            Name = script_name;
            Input = typeof(InArg);
            Output = typeof(OutArg);
            DisplayName = options?.DisplayName ?? Name;
            TargetCount = options?.TargetCount ?? 1;
        }
    }

    public class PipelineStepNullOutput<InArg> : PipelineStep {
        public PipelineStepNullOutput(string script_name, PipelineStepOptions options = null)
        {
            Name = script_name;
            Input = typeof(InArg);
            Output = null;
            DisplayName = options?.DisplayName ?? Name;
            TargetCount = options?.TargetCount ?? 1;
        }
    }

    public class PipelineStepNullInput<OutArg> : PipelineStep {
        public PipelineStepNullInput(string script_name, PipelineStepOptions options = null)
        {
            Name = script_name;
            Output = typeof(OutArg);
            Input = null;
            DisplayName = options?.DisplayName ?? Name;
            TargetCount = options?.TargetCount ?? 1;
        }
    }
}