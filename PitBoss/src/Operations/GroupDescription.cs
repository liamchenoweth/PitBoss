

namespace PitBoss
{
    public class GroupDescription
    {
        public string Name {get; set;}
        public string Script {get; set;}
        public int TargetSize {get;set;}
        public int CurrentSize {get;set;}
        public OperationGroupStatus Status {get;set;}
    }
}