#!/usr/bin/env dotnet-script
//#r "/Projects/PitBoss/PitBoss/bin/Debug/netcoreapp3.1/PitBoss.dll"
#r "/app/PitBoss.dll"
using PitBoss;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

class Step : IOperation<List<int>, int>
{
    public int Execute(List<int> input)
    {
        Random r = new Random();
        //Task.Delay(input * 10000).Wait();
        if(true)//r.NextDouble() > 0.5)
        {
            throw new Exception("Test Exception");
        }
        return input.Sum();
    }
}