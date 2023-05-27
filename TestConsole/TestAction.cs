using System;
using System.Collections.Generic;
using GoryMoon.StreamEngineer.Data;
using Newtonsoft.Json;

namespace TestConsole;

internal class TestAction : BaseAction
{
    [JsonIgnore]
    public new static string TypeName => "meteors";
    
    public double Radius { get; set; }
    public string Amount { get; set; }

    public override void Execute(Data data, Dictionary<string, object> parameters)
    {
        Console.WriteLine(ToString());
        Console.WriteLine(GetEventValue(Amount, 1, parameters));
    }

    public override string ToString()
    {
        return base.ToString() + $" {nameof(Radius)}: {Radius}, {nameof(Amount)}: {Amount}";
    }
}