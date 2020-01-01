using Microsoft.Extensions.Configuration;

namespace PitBoss {
    public class Boss : IBoss {

        private IPipelineManager _pipelineManager;
        private IConfiguration _configuration;

        public Boss(IPipelineManager pipelineManager, IConfiguration configuration) 
        {
            _pipelineManager = pipelineManager;
            _configuration = configuration;
            OnStartUp();
        }

        private void OnStartUp()
        {
            _pipelineManager.CompilePipelines(_configuration["Boss:Scripts:Location"]);
        }
    }
}