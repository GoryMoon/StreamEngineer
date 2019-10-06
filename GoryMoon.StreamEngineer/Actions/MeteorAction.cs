using System;
using GoryMoon.StreamEngineer.Data;

namespace GoryMoon.StreamEngineer.Actions
{
    public class MeteorAction : BaseAction
    {
        
        public float Radius { get; set; }
        public string Amount { get; set; }
        
        public override void Execute(Data.Data data)
        {
            double amount = GetEventValue(Amount, 1, data);
            SessionHandler.EnqueueMeteors((int) Math.Ceiling(amount), Radius);
        }
    }
}