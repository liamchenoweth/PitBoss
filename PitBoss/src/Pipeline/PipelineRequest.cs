using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PitBoss
{
    public class PipelineRequest {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id {get;set;}
        public string PipelineName {get;set;}
        public RequestStatus Status {get;set;}
        public string Input {get;set;}
    }
}