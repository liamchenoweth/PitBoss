#!/usr/bin/env dotnet-script
#r "/Projects/PitBoss/PitBoss/bin/Debug/netcoreapp3.1/PitBoss.dll"
#load "../additions/model.csx"
using PitBoss;

class Builder : IPipelineBuilder {
    public Pipeline Build() {
        PipelineDescriptor desc = new PipelineDescriptor{
            Name = "test"
        };

        var branch1 = (new PipelineBuilder<testOutput>(new PipelineDescriptor{ BranchId = "branch1" })).AddStep(new PipelineStep<testOutput, int>("testIntermediate.csx"));

        var branch3 = (new PipelineBuilder<testOutput>(new PipelineDescriptor{ BranchId = "branch3" })).AddStep(new PipelineStep<testOutput, int>("testIntermediate.csx"))
        .AddStep(new PipelineStep<int, int>("testIntermediate.csx"));

        var branch4 = (new PipelineBuilder<testOutput>(new PipelineDescriptor{ BranchId = "branch4" })).AddStep(new PipelineStep<testOutput, int>("testIntermediate.csx"))
        .AddStep(new PipelineStep<int, int>("testIntermediate.csx"));
        
        var branch2 = (new PipelineBuilder<testOutput>(new PipelineDescriptor{ BranchId = "branch2" })).AddStep(new PipelineStep<testOutput, int>("testIntermediate.csx"))
        .AddStep(new PipelineStep<int, int>("testIntermediate.csx"))
        .AddBranch(new BranchingStep<int, testOutput>("testBranch.csx"), new PipelineStep<int, int>("testBranchOutput.csx"),
            new List<PipelineBuilder<int>>() {branch3, branch4});


        var loop = (new PipelineBuilder<int>(new PipelineDescriptor{ Name = "testLoop"}))
        .AddStep(new PipelineStep<int, int>("testIntermediate.csx"))
        .AddStep(new PipelineStep<int, int>("testIntermediate.csx"));

        var builder = new PipelineBuilder<int>(desc);

        return builder
            .AddStep(new PipelineStep<int, IEnumerable<int>>("generatePopulation.csx"))
            .AddStep(new PipelineStep<IEnumerable<int>, evaluatePopOutput>("evaluatePopulation.csx"))
            .AddBranch(new BranchingStep<evaluatePopOutput, testOutput>("testBranch.csx"),
                new PipelineStep<int, int>("testBranchOutput.csx"), 
                new List<PipelineBuilder<int>>() { branch1, branch2 })
            .AddLoop<int, testLoopOutput>(loop, new LoopStep<int, testLoopOutput>("testBranchOutput.csx"))
            .Build();


    }
}
