#!/usr/bin/env dotnet-script
#r "../../../PitBoss/bin/Debug/netcoreapp3.1/PitBoss.dll"
using PitBoss;

class Step : IOperation<int, string>
{
    public string Execute(int input)
    {
        return input.ToString();
    }
}