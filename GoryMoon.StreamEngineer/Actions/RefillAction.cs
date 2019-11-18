using System.Collections.Generic;
using GoryMoon.StreamEngineer.Data;
using Sandbox.Game.Entities.Character.Components;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.GameSystems;
using Sandbox.Game.World;
using VRage.Game;
using VRageMath;

namespace GoryMoon.StreamEngineer.Actions
{
    public class RefillAction: BaseAction
    {
        public override void Execute(Data.Data data, Dictionary<string, object> parameters)
        {
            SessionHandler.EnqueueAction(() =>
            {
                var player = Utils.GetPlayer();
                if (player != null)
                {
                    var character = player.Character;
                    var health = character.StatComp.Health;
                    if (health != null)
                    {
                        health.Value = health.MaxValue;
                    }

                    if (MySession.Static.Settings.EnableOxygen)
                    {
                        character.OxygenComponent.SuitOxygenAmount =
                            character.OxygenComponent.OxygenCapacity;
                    }

                    var myDefinitionId = MyCharacterOxygenComponent.HydrogenId;
                    character.OxygenComponent.UpdateStoredGasLevel(ref myDefinitionId, 1.0f);

                    character.SuitBattery.ResourceSource.SetRemainingCapacityByType(
                    MyResourceDistributorComponent.ElectricityId, 1E-05f);
                }
            });
        }
    }
}