#!/usr/bin/env dotnet-script
#r "/Projects/PitBoss/PitBoss/bin/Debug/netcoreapp3.1/PitBoss.dll"
using System.Collections.Generic;
using PitBoss;

class evaluatePopOutput
{
    public IEnumerable<int> population;
    public int fit;
}

class testOutput : IBranchResponse
{
    public string BranchId {get;set;}
}

class testLoopOutput : ILoopEnd
{
    public bool Loop {get;set;}
}