using System.Collections.Generic;
using GoryMoon.StreamEngineer.Data;
using Sandbox.Game.Entities;
using VRage;
using VRage.ObjectBuilders;

namespace GoryMoon.StreamEngineer.Actions
{
    public class FulfillBuildPlannerAction: BaseAction
    {
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