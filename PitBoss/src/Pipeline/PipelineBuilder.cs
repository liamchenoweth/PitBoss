using System;
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

        private string getStepId(PipelineStep step)
        {
            return $"{_description.Name}-{step.Name}-{_steps.Count}-{_description.BranchId ?? "0"}";
        }

        public PipelineBuilder<TOut> AddStep<TOut>(PipelineStep<TIn, TOut> step) {
            step.SetStepId(getStepId(step));
            var lastStep = _steps.LastOrDefault();
            if(lastStep != default && lastStep.NextSteps != null) step.AddAsNextSteps(lastStep.NextSteps);
            step.AddStepsToPipeline(_steps);
            return new PipelineBuilder<TOut>(this);
        }

        public override Pipeline Build() {
            if(_steps.Select(x => x.Id).Distinct().Count() != _steps.Count) throw new Exception($"Pipeline {_description.Name} has steps with duplicate IDs.\n please make sure all builders have distinct names and branch IDs");
            return new Pipeline(_description, _steps);
        }

        public PipelineBuilder<TOut> AddBranch<TOut, TInter, TOuter>(BranchingStep<TIn, TInter> inStep, PipelineStep<TOuter, TOut> outStep, IEnumerable<PipelineBuilder<TOuter>> branches) where TInter : IBranchResponse
        {
            inStep.SetStepId(getStepId(inStep));
            outStep.SetStepId(getStepId(outStep));
            branches.Select(x => x._steps.First()).ToList().ForEach(x => x.AddAsNextSteps(inStep.NextSteps));
            branches.Select(x => x._steps.Last()).ToList().ForEach(x => outStep.AddAsNextSteps(x.NextSteps));
            inStep.PipelineChoices(branches.Select(x => (PipelineBuilder)x).ToList());
            inStep.SetBranchEnd(outStep);
            var builder = AddStep(inStep);
            builder._steps.AddRange(branches.SelectMany(x => x._steps));
            builder._steps.Add(outStep);

            return new PipelineBuilder<TOut>(builder);
        }

        public PipelineBuilder<TOut> AddLoop<TOuter, TOut>(PipelineBuilder<TOuter> loop, LoopStep<TOuter, TOut> loopStep) where TOut : ILoopEnd
        {
            var lastStep = _steps.Last();
            var firstStep = loop._steps.First();
            firstStep.AddAsNextSteps(lastStep.NextSteps);
            loopStep.SetLoopStart(firstStep);
            var builder = loop.AddStep(loopStep);
            _steps.AddRange(builder._steps);
            return new PipelineBuilder<TOut>(this);
        }
    }
}