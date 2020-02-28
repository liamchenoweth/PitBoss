using System.Linq;
using System.Text.Json.Serialization;

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

        [JsonIgnore]
        public bool Healthy {
            get
            {
                return HealthyStatus.Contains(ContainerStatus);
            }
        }
    }
    
}