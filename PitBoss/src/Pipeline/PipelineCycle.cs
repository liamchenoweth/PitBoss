using System.Collections.Generic;

namespace PitBoss {
    public abstract class CycleDiscriminator {

    }

    public abstract class PipelineCycle {
        internal List<PipelineStep> _steps;

    }

    public class PipelineCycle<TIn, TOut> : PipelineCycle {
        public PipelineCycle() {

        }

        private PipelineCycle(PipelineCycle copy) {
            
        }

        public PipelineCycle<TIn, SOut> AddStep<SOut>(PipelineStep<TOut, SOut> step) {
            return new PipelineCycle<TIn, SOut>();
        }

        public PipelineCompleteCycle<TIn, SOut> AddDiscriminator<SOut>(PipelineStep<TOut, SOut> step) where SOut : CycleDiscriminator {
            return new PipelineCompleteCycle<TIn, SOut>(AddStep(step));
        }
    }

    public sealed class PipelineCompleteCycle<TIn, TOut> : PipelineCycle {
        internal PipelineCompleteCycle(PipelineCycle<TIn, TOut> builtCycle) {

        }
    }
}