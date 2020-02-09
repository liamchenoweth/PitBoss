using System.Linq;

namespace PitBoss
{
    public enum ContainerStatus
    {
        None,
        Ready,
        Processing,
        Waiting,
        ShuttingDown,
        Error
    }

    public class OperationStatus {
        private ContainerStatus[] HealthyStatus = { ContainerStatus.Ready, ContainerStatus.Processing, ContainerStatus.Waiting };
        public ContainerStatus ContainerStatus { get; set; }

        public bool Healthy {
            get
            {
                return HealthyStatus.Contains(ContainerStatus);
            }
        }
    }
    
}