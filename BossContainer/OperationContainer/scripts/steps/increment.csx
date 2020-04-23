#!/usr/bin/env dotnet-script
#r "/Projects/PitBoss/PitBoss/bin/Debug/netcoreapp3.1/PitBoss.dll"
using PitBoss;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class Step : IOperation<int, int>
{
    public int Execute(int input)
    {
        Task.Delay(input * 10000).Wait();
        return input + 1;
    }
}