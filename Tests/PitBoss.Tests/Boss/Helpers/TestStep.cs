using PitBoss;

namespace PitBoss.Tests
{
    public class TestStep<InArg, OutArg> : PipelineStep<InArg, OutArg>
    {
        public TestStep(string script_name, PipelineStepOptions options = null) : base(script_name, options)
        {

        }

        public void SetId(string id)
        {
            Id = id;
        }
    }
}