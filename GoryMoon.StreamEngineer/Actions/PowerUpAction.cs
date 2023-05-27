using System.Collections.Generic;
using GoryMoon.StreamEngineer.Data;
using Newtonsoft.Json;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;

namespace GoryMoon.StreamEngineer.Actions
{
    public class PowerUpAction: BaseAction
    {
        [JsonIgnore]
        public new static string TypeName => "power_up";
        
        public override void Execute(Data.Data data, Dictionary<string, object> parameters)
        {
            SessionHandler.EnqueueAction(() =>
            {
                var player = Utils.GetPlayer();
                if (player == null) return;
                var controlledEntity = player.Controller.ControlledEntity;

                if (controlledEntity is MyShipController controller)
                {
                    var blocks = controller.CubeGrid.GetFatBlocks<MyBatteryBlock>();
                    foreach (var block in blocks)
                    {
                        block.CurrentStoredPower = block.MaxStoredPower;
                    }
                }
                player.Character.SuitBattery.ResourceSource.SetRemainingCapacityByType(
                    MyResourceDistributorComponent.ElectricityId, 1E-05f);
            });
        }
    }
}