using System.Collections.Generic;
using System.Threading.Tasks;

namespace PitBoss {
    public interface IPipelineManager
    {
        bool Ready { get; }
        IEnumerable<Pipeline> Pipelines { get; }
        IEnumerable<Pipeline> CompilePipelines(string directory);
        Task<IEnumerable<Pipeline>> CompilePipelinesAsync(string directory);
        Pipeline GetPipeline(string name);
        PipelineModel GetPipelineVersion(string version);
        void RegisterPipelines();
        void ClearPipelines();
        //PipelineStep GetPipelineStep(string pipeline, string pipelineName);
    }
}