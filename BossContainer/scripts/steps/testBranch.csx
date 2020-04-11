#!/usr/bin/env dotnet-script
#r "/Projects/PitBoss/PitBoss/bin/Debug/netcoreapp3.1/PitBoss.dll"
#load "../additions/model.csx"
using PitBoss;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class Step : IOperation<evaluatePopOutput, testOutput>
{
    public testOutput Execute(evaluatePopOutput input)
    {
        return new testOutput {
            BranchId = "branch1"
        };
    }
}