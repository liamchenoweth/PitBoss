namespace PitBoss {
    public interface IBoss
    {
        void ProcessRequest(JobRequest request);
        void GeneratePipelineDefinitions();
        void GenerateOperations();
        void CreateContainers(Pipeline pipeline);
    }
}