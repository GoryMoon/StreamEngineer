using System.Collections.Generic;
using GoryMoon.StreamEngineer.Data;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character.Components;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.GameSystems;
using Sandbox.Game.World;
using VRage.Game;
using VRageMath;

namespace GoryMoon.StreamEngineer.Actions
{
    public class PowerUpAction: BaseAction
    {
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