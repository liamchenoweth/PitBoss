#!/usr/bin/env dotnet-script
#r "/Projects/PitBoss/PitBoss/bin/Debug/netcoreapp3.1/PitBoss.dll"
#load "../additions/model.csx"
using PitBoss;

class Builder : IPipelineBuilder {
    public Pipeline Build() {
        PipelineDescriptor desc = new PipelineDescriptor{
            Name = "test"
        };

        PipelineBuilder<int> builder = new PipelineBuilder<int>(desc);
        return builder
            .AddStep(new PipelineStep<int, IEnumerable<int>>("generatePopulation.csx"))
            .AddStep(new PipelineStep<IEnumerable<int>, evaluatePopOutput>("evaluatePopulation.csx"))
            .Build();
    }
}
