namespace PitBoss
{
    public class OperationGroupStatus
    {
        public int Containers {get;set;}
        public int HealthyContainers {get;set;}
        public int UnhealthyContainers {get;set;}
        public int ReadyContainers {get;set;}
        public int ProcessingContainers {get;set;}
    }
    
}