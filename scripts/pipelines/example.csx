#!/usr/bin/env dotnet-script
//#r "/Projects/PitBoss/PitBoss/bin/Debug/netcoreapp3.1/PitBoss.dll"
#r "/app/PitBoss.dll"
#load "../additions/model.csx"
using PitBoss;

class Builder : IPipelineBuilder {
    public Pipeline Build() {
        PipelineDescriptor desc = new PipelineDescriptor{
            Name = "test"
        };

        var distribution1 = new PipelineBuilder<int>(new PipelineDescriptor{
            Name = "plus1"
        })
        .AddStep(new PipelineStep<int, int>("increment.csx", new PipelineStepOptions(){
            DisplayName = "TestName",
            TargetCount = 10
        }))
        .AddStep(new PipelineStep<int, int>("increment.csx"));

        return new PipelineBuilder<int>(desc)
        .AddStep(new PipelineStep<int, List<int>>("generatePopulation.csx"))
        .AddDistributedSection(distribution1)
        .Build();
    }
}
