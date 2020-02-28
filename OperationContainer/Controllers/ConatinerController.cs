using Microsoft.AspNetCore.Mvc;

namespace PitBoss {
    public class ContainerController : Controller {
        private IOperationHealthManager _healthManager;
        public ContainerController(IOperationHealthManager healthManager) {
            _healthManager = healthManager;
        }

        [HttpGet("container/status")]
        public OperationStatus ContainerStatus() {
            return _healthManager.GetContainerStatus();
        }
    }
}