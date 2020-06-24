using Newtonsoft.Json;

namespace PitBoss {
    public class OperationResponse : BaseEntity
    {
        public OperationResponse() {}
        public OperationResponse(OperationRequest request)
        {
            Id = request.Id;
            PipelineId = request.PipelineId;
            PipelineName = request.PipelineName;
            PipelineStepId = request.PipelineStepId;
        }

        public OperationResponse(OperationResponse response)
        {
            Id = response.Id;
            PipelineId = response.PipelineId;
            PipelineName = response.PipelineName;
            PipelineStepId = response.PipelineStepId;
            Success = response.Success;
            _result = response._result;
            Created = response.Created;
            Updated = response.Updated;
        }

        public string Id {get; set;}
        public string PipelineId { get; set; }
        public string PipelineName { get; set; }
        public string PipelineStepId { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
        
        protected string _result;
        public string Result {
            get => _result;
            set => _result = value;
        }
    }

    public class OperationResponse<T> : OperationResponse 
    {
        public OperationResponse() : base () {}
        public OperationResponse(OperationRequest request) : base(request) {}
        public OperationResponse(OperationResponse response) : base(response) {}

        [JsonIgnore]
        public new T Result {
            get
            {
                return JsonConvert.DeserializeObject<T>(_result);
            }
            set
            {
                _result = JsonConvert.SerializeObject(value);
            }
        }
    }
}