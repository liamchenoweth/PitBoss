using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PitBoss.Utils;

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
            Version = FileUtils.Sha256Hash(string.Join('.', _steps.Select(x => x.ToModel().HashCode)));
        }

        public List<PipelineStep> Steps 
        { 
            get
            {
                return _steps;
            } 
        }

        public string Name { get => Description.Name; }
        public string Version { get; private set; }
        public string DllLocation { get; set; }

        public PipelineModel ToModel()
        {
            var model = new PipelineModel()
            {
                Name = Name,
                Version = Version
            };
            model.Steps = _steps.Select(x => x.ToModel()).Select(x => new PipelineToStepMapper{ Pipeline = model, Step = x }).ToList();
            return model;
        }
    }

    public class PipelineModel
    {
        public List<PipelineToStepMapper> Steps { get; set; }
        public string Name { get; set; }
        [Key]
        public string Version { get; set; }
    }
}