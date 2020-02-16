using Microsoft.AspNetCore.Mvc;

namespace PitBoss {
    public class HeartbeatController : Controller {
        public HeartbeatController() {

        }

        [HttpGet("heartbeat")]
        public OperationStatus Heartbeat() {
            return new OperationStatus();
        }
    }
}