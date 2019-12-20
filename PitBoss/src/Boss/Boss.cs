namespace PitBoss {
    public class Boss : IBoss {
        private BossDescriptor _desc;
        public Boss(BossDescriptor desc) {
            _desc = desc;
        }

        public void ProcessRequest(JobRequest request) {

        }

        public void GeneratePipelineDefinitions() {

        }

        public void GenerateOperations() {

        }

        public void CreateContainers(Pipeline pipeline) {

        }
    }
}