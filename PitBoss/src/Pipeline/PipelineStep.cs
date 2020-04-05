using System;
using System.Linq;
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
    }

    public class PipelineStep<InArg, OutArg> : PipelineStep {

        internal PipelineStep() {
            Input = typeof(InArg);
            Output = typeof(OutArg);
        }

        public PipelineStep(string script_name) {
            Name = script_name;
            Input = typeof(InArg);
            Output = typeof(OutArg);
        }
    }

    public class PipelineStepNullOutput<InArg> : PipelineStep {
        public PipelineStepNullOutput(string script_name)
        {
            Name = script_name;
            Input = typeof(InArg);
            Output = null;
        }
    }

    public class PipelineStepNullInput<OutArg> : PipelineStep {
        public PipelineStepNullInput(string script_name)
        {
            Name = script_name;
            Output = typeof(OutArg);
            Input = null;
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

    public class DistributedStep<InArg, OutArg> : PipelineStep<InArg, OutArg> where InArg : IEnumerable<InArg> {
        public DistributedStep(string script_name) : base(script_name) {}
    }

    public interface IBranchResponse
    {
        string BranchId {get;}
    }

    public class BranchingStep<InArg, OutArg> : PipelineStep<InArg, OutArg> where OutArg : IBranchResponse
    {
        private Dictionary<string, string> _nextPipelines;
        private List<PipelineStep> _subSteps;
        public BranchingStep(IEnumerable<PipelineBuilder<OutArg>> pipelines) : base()
        {
            var builtPipelines = pipelines.Select(x => new { desc = x._description, pipe = x.Build() });
            _nextPipelines = builtPipelines.ToDictionary(x => x.desc.BranchId, y => y.pipe.Steps.First().Id);
            _subSteps = builtPipelines.SelectMany(x => x.pipe.Steps).ToList();
        }

        public override string GetNextStep(OperationResponse response)
        {
            var properResponse = response as OperationResponse<OutArg>;
            if(_nextPipelines.TryGetValue(properResponse.Result.BranchId, out var nextId))
            {
                return nextId;
            }
            throw new KeyNotFoundException($"Step name \"{properResponse.Result.BranchId}\" not found");
        }

        public override void AddAsNextSteps(List<string> steps)
        {
            steps.AddRange(_nextPipelines.Values);
        }

        public override void AddStepsToPipeline(List<PipelineStep> steps)
        {
            steps.AddRange(_subSteps);
        }
    }
}