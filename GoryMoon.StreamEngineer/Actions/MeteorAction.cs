using System;
using System.Collections.Generic;
using GoryMoon.StreamEngineer.Data;
using Newtonsoft.Json;

namespace GoryMoon.StreamEngineer.Actions
{
    public class MeteorAction : BaseAction
    {
        [JsonIgnore]
        public new static string TypeName => "meteors";
        
        public float Radius { get; set; }
        public string Amount { get; set; }

        public override void Execute(Data.Data data, Dictionary<string, object> parameters)
        {
            var amount = GetEventValue(Amount, 1, parameters);
            SessionHandler.EnqueueMeteors((int) Math.Ceiling(amount), Radius);
        }
    }
}