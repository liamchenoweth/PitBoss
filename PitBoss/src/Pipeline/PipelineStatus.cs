using System.Collections.Generic;
using PitBoss.Utils;

namespace PitBoss
{
    public class PipelineStatus
    {
        public string PipelineName { get; set; }
        public IEnumerable<PipelineStepStatus> Steps {get; set;}
        public Health Health {get; set;}
    }

    public class PipelineStepStatus
    {
        public string PipelineStepName {get; set;}
        public Health Health {get; set;}
    }
}