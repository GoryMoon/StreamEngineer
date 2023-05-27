using System;
using System.Collections.Generic;
using GoryMoon.StreamEngineer.Data;
using Newtonsoft.Json;

namespace TestConsole;

internal class SnapAction : BaseAction
{
    [JsonIgnore]
    public new static string TypeName => "snap";
    
    public bool Vehicle { get; set; } = true;

    [JsonProperty("vehicle_percentage")] public double VehiclePercentage { get; set; } = 0.5;
    [JsonProperty("player_percentage")] public double PlayerPercentage { get; set; } = 0.5;
    
    public override void Execute(Data data, Dictionary<string, object> parameters)
    {
        Console.WriteLine(ToString());
    }

    public override string ToString()
    {
        return
            $"{base.ToString()}, {nameof(Vehicle)}: {Vehicle}, {nameof(VehiclePercentage)}: {VehiclePercentage}, {nameof(PlayerPercentage)}: {PlayerPercentage}";
    }
}