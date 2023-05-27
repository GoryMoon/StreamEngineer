using System;
using System.Collections.Generic;
using GoryMoon.StreamEngineer.Data;
using Newtonsoft.Json;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;

namespace GoryMoon.StreamEngineer.Actions
{
    public class PowerDownAction: BaseAction
    {
        [JsonIgnore]
        public new static string TypeName => "power_down";
        
        public string Amount { get; set; }
        public override void Execute(Data.Data data, Dictionary<string, object> parameters)
        {
            SessionHandler.EnqueueAction(() =>
            {
                var player = Utils.GetPlayer();
                if (player == null) return;
                var character = player.Character;
                var controlledEntity = player.Controller.ControlledEntity;

                if (controlledEntity is MyShipController controller)
                {
                    var eventValue = GetEventValue(Amount, -1, parameters);
                    var lower = Math.Abs(eventValue - (-1)) < Double.Epsilon;
                        
                    var blocks = controller.CubeGrid.GetFatBlocks<MyBatteryBlock>();
                    foreach (var block in blocks)
                    {
                        if (lower)
                        {
                            var change = block.CurrentStoredPower - eventValue;
                            block.CurrentStoredPower = (float) Math.Max(0.0, block.CurrentStoredPower - eventValue);
                            if (change >= 0) return;

                            eventValue -= block.CurrentStoredPower;
                        }
                        else
                        {
                            block.CurrentStoredPower = 0;
                        }
                        controller.CubeGrid.SetCubeDirty(block.Position);
                    }
                }
                    
                character.SuitBattery.ResourceSource.SetRemainingCapacityByType(
                    MyResourceDistributorComponent.ElectricityId, 2E-07F); // 2% left
            });
        }
    }
}