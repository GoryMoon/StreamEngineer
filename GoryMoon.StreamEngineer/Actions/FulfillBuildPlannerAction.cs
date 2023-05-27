using System.Collections.Generic;
using GoryMoon.StreamEngineer.Data;
using Newtonsoft.Json;

namespace GoryMoon.StreamEngineer.Actions
{
    public class FulfillBuildPlannerAction: BaseAction
    {
        [JsonIgnore]
        public new static string TypeName => "fulfill_buildplanner";

        public override void Execute(Data.Data data, Dictionary<string, object> parameters)
        {
            SessionHandler.EnqueueAction(() =>
            {
                var player = Utils.GetPlayer();
                if (player != null)
                {
                    var character = player.Character;
                    var matrix = character.WorldMatrix;
                    
                    foreach (var planItem in character.BuildPlanner)
                    {
                        foreach (var component in planItem.Components)
                        {
                            var amount = (double) component.Count;
                            Utils.AddOrDropItem(player, component.ComponentDefinition, ref amount, matrix);
                            component.Count = (int) amount;
                        }

                        planItem.Components.RemoveAll(component => component.Count <= 0);
                    }
                    character.CleanFinishedBuildPlanner();
                }
            });
        }
    }
}