using System;
using System.Collections.Generic;

namespace PitBoss {
    public class Pipeline {

        private List<PipelineStep> _steps;

        public Pipeline(List<PipelineStep> steps) {
            _steps = steps;
        }

        internal List<PipelineStep> Steps { get; }
    }
}