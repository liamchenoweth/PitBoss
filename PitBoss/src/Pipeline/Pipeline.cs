using System;
using System.Collections.Generic;

namespace PitBoss {
    public class Pipeline {

        private List<PipelineStep> _steps;
        private PipelineDescriptor _desc;

        public Pipeline(PipelineDescriptor desc, List<PipelineStep> steps) {
            _steps = steps;
            _desc = desc;
        }

        public Pipeline() {
            throw new NotImplementedException("FOR STUBBING ONLY");
        }

        internal List<PipelineStep> Steps { get; }

        public string Name { get => _desc.Name; }
        public string DllLocation { get; set; }
    }
}