using System.Collections.Generic;

namespace PitBoss {
    public interface IPipelineManager
    {
        List<Pipeline> Pipelines { get; }
        List<Pipeline> CompilePipelines(string directory);
        Pipeline GetPipeline(string name);

        
    }
}