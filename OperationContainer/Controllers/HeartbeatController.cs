using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace PitBoss {
    public class HeartbeatController : Controller {
        private IHostApplicationLifetime _lifetime;
        private ContainerController _containerController;
        public HeartbeatController(IHostApplicationLifetime lifetime, ContainerController containerController) {
            _lifetime = lifetime;
            _containerController = containerController;
        }

        [HttpGet("heartbeat")]
        public ActionResult Heartbeat() {
            return Ok();
        }

        [HttpPost("shutdown")]
        public ActionResult Shutdown()
        {
            JobContainer.Job.serviceToken.Cancel();
            return Ok(_containerController.ContainerStatus());
        }
    }
}