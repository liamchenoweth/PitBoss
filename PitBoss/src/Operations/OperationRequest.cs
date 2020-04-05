using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PitBoss {
    public class OperationRequest : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id {get; set;}
        public string PipelineName { get; set; }
        public string PipelineId { get; set; }
        public string PipelineStepId { get; set; }
        public string CallbackUri { get; set; }
        public RequestStatus Status { get; set; }
        public DateTime Started {get;set;}
        public DateTime Completed {get;set;}
        public OperationRequest() {}
        public OperationRequest(PipelineRequest pipeline, string step)
        {
            PipelineName = pipeline.PipelineName;
            PipelineId = pipeline.Id;
            PipelineStepId = step;
        }
    }

    public class OperationRequest<T> : OperationRequest 
    {    
        public T Parameter {get; set;}
        public OperationRequest() {}
        public OperationRequest(OperationRequest request, T parameter)
        {
            Parameter = parameter;
            Id = request.Id;
            PipelineName = request.PipelineName;
            PipelineId = request.PipelineId;
            PipelineStepId = request.PipelineStepId;
            CallbackUri = request.CallbackUri;
            Status = request.Status;
        }
    }

    public class OperationRequestStatus
    {
        public string RequestId {get; set;}
        public RequestStatus Status {get;set;}
    }
}