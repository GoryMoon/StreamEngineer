using System;
using System.Collections.Generic;
using System.Linq;
using GoryMoon.StreamEngineer.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Sandbox.Game.Entities;
using VRage.Game.ModAPI;
using VRageMath;

namespace GoryMoon.StreamEngineer.Actions
{
    [JsonConverter(typeof(StringEnumConverter), typeof(SnakeCaseNamingStrategy))]
    public class SnapAction: BaseAction
    {
        public bool Vehicle { get; set; } = true;
        
        public double VehiclePercentage { get; set; } = 0.5;
        public double PlayerPercentage { get; set; } = 0.5;
        
        private readonly Random _random = new Random();
        
        public override void Execute(Data.Data data, Dictionary<string, object> parameters)
        {
            SessionHandler.EnqueueAction(() =>
            {
                var player = Utils.GetPlayer();
                if (player != null)
                {
                    if (Vehicle)
                    {
                        var controlledEntity = player.Controller.ControlledEntity;
                        if (controlledEntity is MyShipController controller)
                        {
                            var grid = controller.CubeGrid;
                            grid.CubeBlocks.Where(block => _random.NextDouble() < VehiclePercentage).ToList()
                                .ForEach(block => grid.RemoveBlock(block));
                        }
                    }

                    if (_random.NextDouble() < PlayerPercentage)
                    {
                        player.Character.Kill(true, new MyDamageInformation());
                    }
                    ActionNotification.SendActionMessage("SNAP!", Color.Red, "ArcHudGPSNotification1");
                }
            });
        }
    }
}