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
        // How to handle this will get confusing
        // Want to handle this on the operation as it will be much better than on the boss
        // TODO: add this back
        // IDEA: instead of operation returning value, instead return the operationResponse?
        // Or some other object that contains all the options we want to expose
        //public bool Loop { get; set; }

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