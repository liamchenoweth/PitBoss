namespace PitBoss {
    public class OperationResponse
    {
        public OperationResponse() {}
        public OperationResponse(OperationRequest request)
        {
            Id = request.Id;
            PipelineId = request.PipelineId;
        }
        public string Id {get; set;}
        public string PipelineId { get; set; }
        public string PipelineName { get; set; }
        public int PipelineStepId { get; set; }
        public bool Success { get; set; }
        // How to handle this will get confusing
        // Want to handle this on the operation as it will be much better than on the boss
        // TODO: add this back
        // IDEA: instead of operation returning value, instead return the operationResponse?
        // Or some other object that contains all the options we want to expose
        //public bool Loop { get; set; }
    }

    public class OperationResponse<T> : OperationResponse 
    {    
        public T Result {get; set;}
    }
}