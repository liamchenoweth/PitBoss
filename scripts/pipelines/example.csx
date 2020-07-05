#!/usr/bin/env dotnet-script
//#r "/Projects/PitBoss/PitBoss/bin/Debug/netcoreapp3.1/PitBoss.dll"
#r "/app/PitBoss.dll"
#load "../additions/model.csx"
using PitBoss;

class Builder : IPipelineBuilder {
    public Pipeline Build() {
        PipelineDescriptor desc = new PipelineDescriptor{
            Name = "test",
            RetryStrategy = RetryStrategy.Linear
        };

        var distribution1 = new PipelineBuilder<int>(new PipelineDescriptor{
            Name = "plus1"
        })
        .AddStep(new PipelineStep<int, int>("increment.csx", new PipelineStepOptions(){
            DisplayName = "TestName",
            TargetCount = 10
        }))
        .AddStep(new PipelineStep<int, int>("increment.csx"))
        .AddStep(new PipelineStep<int, int>("increment.csx"));

        return new PipelineBuilder<int>(desc)
        .AddStep(new PipelineStep<int, List<int>>("generatePopulation.csx"))
        .AddStep(new PipelineStep<List<int>, evaluatePopOutput>("evaluatePopulation.csx"))
        // .AddDistributedSection(distribution1)
        // .AddStep(new PipelineStep<IEnumerable<int>, int>("ThrowError.csx"))
        // .AddStep(new PipelineStep<int, int>("increment.csx"))
        .Build();
    }
}
