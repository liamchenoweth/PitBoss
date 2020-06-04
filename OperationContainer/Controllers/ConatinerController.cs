using Microsoft.AspNetCore.Mvc;

namespace PitBoss {
    public class ContainerController : Controller {
        private IOperationHealthManager _healthManager;
        private IOperationManager _manager;
        public ContainerController(IOperationHealthManager healthManager, IOperationManager manager) {
            _healthManager = healthManager;
            _manager = manager;
        }

        [HttpGet("container/status")]
        public OperationStatus ContainerStatus() {
            var status = _healthManager.GetContainerStatus();
            if(!_manager.Ready) status.ContainerStatus = PitBoss.ContainerStatus.Waiting;
            return status;
        }
    }
}