using System.Linq;

namespace PitBoss
{
    public interface ILoopEnd
    {
        bool Loop {get;}
    }
    public class LoopStep<TIn, TOut> : PipelineStep<TIn, TOut> where TOut : ILoopEnd
    {
        public string LoopStart {get;private set;}
        public LoopStep(string script, PipelineStepOptions options = null) : base(script, options)
        {
        }

        internal void SetLoopStart(PipelineStep step)
        {
            LoopStart = step.Id;
            NextSteps.Add(step.Id);
        }

        public override string GetNextStep(OperationResponse response)
        {
            var properResponse = response as OperationResponse<TOut>;
            if(properResponse.Result.Loop) return LoopStart;
            return NextSteps.Where(x => x != LoopStart).FirstOrDefault();
        }
    }
}