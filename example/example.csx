#!/usr/bin/env dotnet-script
#r "../PitBoss/bin/Debug/netcoreapp3.1/PitBoss.dll"
using PitBoss;

class Builder : IPipelineBuilder {
    public PipelineBuilder Build() {
        PipelineDescriptor desc = new PipelineDescriptor{
            Name = "test"
        };

        PipelineBuilder<int> builder = new PipelineBuilder<int>(desc);
        return builder.AddStep(new PipelineStep<int, string>("stringify.csx"));
    }
}
