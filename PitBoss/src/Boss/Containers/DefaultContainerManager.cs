using System.Collections.Generic;

namespace PitBoss {
    public class DefaultContainerManager : IContainerManager {
        private List<Container> _containers;

        public DefaultContainerManager() {

        }

        public Container CreateContainer(PipelineStep step) {
            var container = new Container();
            _containers.Add(container);
            return container;
        }

        
    }
}