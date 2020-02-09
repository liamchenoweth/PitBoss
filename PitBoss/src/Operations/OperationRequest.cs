namespace PitBoss {
    public class OperationRequest 
    {
        public string Id {get; set;}
        public string PipelineName { get; set; }
        public string PipelineId { get; set; }
        public int PipelineStepId { get; set; }
        public string CallbackUri { get; set; }
        public RequestStatus Status { get; set; }
        public OperationRequest() {}
        public OperationRequest(PipelineRequest pipeline, int step)
        {
            PipelineName = pipeline.PipelineName;
            PipelineId = pipeline.Id;
            PipelineStepId = step;
        }
    }

    public class OperationRequest<T> : OperationRequest 
    {    
        public T Parameter {get; set;}
    }
}