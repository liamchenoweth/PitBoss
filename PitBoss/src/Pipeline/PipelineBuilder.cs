using System.Linq;
using System.Collections.Generic;

namespace PitBoss {
    public interface IPipelineBuilder
    {
        Pipeline Build();
    }

    public abstract class PipelineBuilder {
        internal List<PipelineStep> _steps;
        internal PipelineDescriptor _description;
        public abstract Pipeline Build();
    }

    public class PipelineBuilder<TIn> : PipelineBuilder {


        public PipelineBuilder(PipelineDescriptor descritpion) {
            _description = descritpion;
            _steps = new List<PipelineStep>();
        }

        private PipelineBuilder(PipelineBuilder copy) {
            _description = copy._description;
            _steps = copy._steps;
        }

        public PipelineBuilder<TOut> AddStep<TOut>(PipelineStep<TIn, TOut> step) {
            var lastStep = _steps.LastOrDefault();
            lastStep?.NextSteps?.Add(step.Id);
            _steps.Add(step);
            return new PipelineBuilder<TOut>(this);
        }

        public override Pipeline Build() {
            return new Pipeline(_description, _steps);
        }
    }
}