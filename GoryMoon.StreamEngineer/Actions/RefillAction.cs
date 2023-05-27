using System.Collections.Generic;
using GoryMoon.StreamEngineer.Data;
using Newtonsoft.Json;
using Sandbox.Game.Entities.Character.Components;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.World;

namespace GoryMoon.StreamEngineer.Actions
{
    public class RefillAction: BaseAction
    {
        [JsonIgnore]
        public new static string TypeName => "refill";
        
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