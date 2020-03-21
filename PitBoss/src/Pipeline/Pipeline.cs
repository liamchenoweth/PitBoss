using System;
using System.Linq;
using System.Collections.Generic;

namespace PitBoss {
    public class Pipeline {

        private List<PipelineStep> _steps;
        public PipelineDescriptor Description {get; private set;}
        public Type Input {get; private set;}
        public Type Output {get; private set;}

        public Pipeline(PipelineDescriptor desc, List<PipelineStep> steps) {
            _steps = steps;
            Description = desc;
            Input = _steps.First().GetType().GenericTypeArguments.First();
            Output = _steps.Last().GetType().GenericTypeArguments.Length == 2 ? _steps.Last().GetType().GenericTypeArguments.Last() : null;
        }

        public List<PipelineStep> Steps 
        { 
            get
            {
                return _steps;
            } 
        }

        public string Name { get => Description.Name; }
        public string DllLocation { get; set; }
    }
}