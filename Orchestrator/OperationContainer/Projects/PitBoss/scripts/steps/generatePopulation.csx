#!/usr/bin/env dotnet-script
#r "/Projects/PitBoss/PitBoss/bin/Debug/netcoreapp3.1/PitBoss.dll"
using PitBoss;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class Step : IOperation<int, IEnumerable<int>>
{
    public IEnumerable<int> Execute(int input)
    {
        var output = new List<int>();
        Random rnd = new Random();
        for(var x = 0; x < input; x++)
        {
            output.Add(rnd.Next(1, 10));
        }
        return output;
    }
}