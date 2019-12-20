using Microsoft.AspNetCore.Mvc;

namespace PitBoss {
    public class HeartbeatController : Controller {
        public HeartbeatController() {

        }

        [HttpGet("/heartbeat")]
        public JobStatus Heartbeat() {
            return new JobStatus();
        }
    }
}