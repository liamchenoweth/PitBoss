using System;
using System.Collections.Generic;

namespace PitBoss {
    public interface IPipelineBuilder
    {
        PipelineBuilder Build();
    }

    public abstract class PipelineBuilder {
        internal List<PipelineStep> _steps;
        internal PipelineDescriptor _description;
    }

    public class PipelineBuilder<TIn> : PipelineBuilder {


        public PipelineBuilder(PipelineDescriptor descritpion) {
            _description = descritpion;
        }

        private PipelineBuilder(PipelineBuilder copy) {
            _description = copy._description;
            _steps = copy._steps;
        }

        public PipelineBuilder<TOut> AddStep<TOut>(PipelineStep<TIn, TOut> step) {
            _steps.Add(step);
            return new PipelineBuilder<TOut>(this);
        }

        public Pipeline Build() {
            return new Pipeline(_steps);
        }
    }
}