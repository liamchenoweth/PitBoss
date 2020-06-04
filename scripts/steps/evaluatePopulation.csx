#!/usr/bin/env dotnet-script
//#r "/Projects/PitBoss/PitBoss/bin/Debug/netcoreapp3.1/PitBoss.dll"
#r "/app/PitBoss.dll"
#load "../additions/model.csx"
using PitBoss;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class Step : IOperation<IEnumerable<int>, evaluatePopOutput>
{
    public evaluatePopOutput Execute(IEnumerable<int> input)
    {
        var score = 0;
        for(var x = 0; x < input.Count(); x++)
        {
            if(x % 2 == 0)
            {
                score += input.ElementAt(x);
            }
            else
            {
                score -= input.ElementAt(x);
            }
        }
        return new evaluatePopOutput {
            population = input,
            fit = score
        };
    }
}