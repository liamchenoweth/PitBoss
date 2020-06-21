using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PitBoss
{
    public class PipelineRequest : BaseEntity {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id {get;set;}
        public string PipelineName {get;set;}
        [ForeignKey("PipelineModel")]
        public string PipelineVersion { get; set; }
        public PipelineModel PipelineModel { get; set; }
        public RequestStatus Status {get;set;}
        public OperationRequest CurrentRequest {get;set;}
        [ForeignKey("Response")]
        public string ResponseId {get;set;}
        public OperationResponse Response {get;set;}
        public string Input {get;set;}
    }
}