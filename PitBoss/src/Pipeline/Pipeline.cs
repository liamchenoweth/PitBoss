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

        internal List<PipelineStep> Steps 
        { 
            get
            {
                return _steps;
            } 
        }

        public string Name { get => _desc.Name; }
        public string DllLocation { get; set; }
    }
}