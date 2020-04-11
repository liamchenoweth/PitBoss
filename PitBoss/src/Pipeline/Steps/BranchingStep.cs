using System.Linq;
using System.Collections.Generic;

namespace PitBoss
{
    public interface IBranchResponse
    {
        string BranchId {get;}
    }
    
    public class BranchingStep<InArg, OutArg> : PipelineStep<InArg, OutArg> where OutArg : IBranchResponse
    {
        private Dictionary<string, string> _nextPipelines;
        public BranchingStep(string script) : base(script)
        {
            IsBranch = true;
            _nextPipelines = new Dictionary<string, string>();
        }

        internal void SetBranchEnd(PipelineStep endStep)
        {
            BranchEndId = endStep.Id;
        }

        internal void PipelineChoices(List<PipelineBuilder> builders)
        {
            foreach(var builder in builders)
            {
                _nextPipelines[builder._description.BranchId] = builder._steps.First().Id;
            }
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
    }
}