using System;
using System.Collections.Generic;
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
        public DateTime Queued { get; set; }
        public DateTime Started {get;set;}
        public DateTime Completed {get;set;}
        public string ParentRequestId {get; set;}
        public string InstigatingRequestId {get;set;}
        public bool IsParentOperation {get;set;}
        public int RetryCount {get; set;}
        public OperationRequest() {}
        public OperationRequest(PipelineRequest pipeline, string step, OperationRequest instigatingRequest)
        {
            PipelineName = pipeline.PipelineName;
            PipelineId = pipeline.Id;
            PipelineStepId = step;
            IsParentOperation = false;
            ParentRequestId = instigatingRequest?.ParentRequestId;
            InstigatingRequestId = instigatingRequest?.Id;
            RetryCount = 0;
        }
    }

    public class OperationRequest<T> : OperationRequest 
    {    
        public T Parameter {get; set;}
        public OperationRequest() {}
        public OperationRequest(OperationRequest request, T parameter, OperationRequest instigatingRequest)
        {
            Parameter = parameter;
            Id = request.Id;
            PipelineName = request.PipelineName;
            PipelineId = request.PipelineId;
            PipelineStepId = request.PipelineStepId;
            CallbackUri = request.CallbackUri;
            Status = request.Status;
            IsParentOperation = false;
            ParentRequestId = request.ParentRequestId;
            InstigatingRequestId = instigatingRequest?.Id;
            RetryCount = 0;
        }
    }

    public class OperationRequestStatus
    {
        public string RequestId {get; set;}
        public RequestStatus Status {get;set;}
    }

    public class DistributedRequestSeed
    {
        public int Id {get; set;}
        public string DistributedOperationRequestId {get;set;}
        public DistributedOperationRequest DistributedOperationRequest {get;set;}
        public string OperationRequestId {get;set;}
        public OperationRequest OperationRequest {get;set;}
    }

    public class DistributedOperationRequest : OperationRequest
    {
        public string EndingStepId {get; set;}
        public string BeginingStepId {get;set;}
        [ForeignKey("DistributedRequestId")]
        public IEnumerable<DistributedRequestSeed> SeedingRequestIds {get;set;}

        public DistributedOperationRequest() : base() {}

        public DistributedOperationRequest(
            PipelineRequest pipeline, 
            string beginingStep, 
            string endingStep,
            OperationRequest instigatingRequest) : base(pipeline, beginingStep, instigatingRequest)
        {
            IsParentOperation = true;
            BeginingStepId = beginingStep;
            EndingStepId = endingStep;
        }
    }
}