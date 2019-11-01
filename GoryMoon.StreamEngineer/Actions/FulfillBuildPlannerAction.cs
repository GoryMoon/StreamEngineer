using GoryMoon.StreamEngineer.Data;
using Sandbox.Game.Entities;
using VRage;
using VRage.ObjectBuilders;

namespace GoryMoon.StreamEngineer.Actions
{
    public class FulfillBuildPlannerAction: BaseAction
    {
        public override void Execute(Data.Data data)
        {
            SessionHandler.EnqueueAction(() =>
            {
                var player = Utils.GetPlayer();
                if (player != null)
                {
                    var character = player.Character;
                    var inventory = character.GetInventory();
                    var matrix = player.Character.WorldMatrix;
                    
                    foreach (var planItem in character.BuildPlanner)
                    {
                        foreach (var component in planItem.Components)
                        {
                            var newObject = MyObjectBuilderSerializer.CreateNewObject(component.ComponentDefinition.Id);

                            var remaining = 0;
                            for (var i = 0; i < component.Count; i++)
                            {
                                if (!inventory.AddItems(1, newObject))
                                {
                                    remaining = component.Count - i;
                                    break;
                                }
                            }
                            component.Count = remaining;
                            if (remaining != 0)
                            {
                                MyFloatingObjects.Spawn(component.ComponentDefinition, matrix.Translation, matrix.Forward, matrix.Up, component.Count);
                                component.Count = 0;
                            }
                        }

                        planItem.Components.RemoveAll(component => component.Count <= 0);
                    }
                    character.CleanFinishedBuildPlanner();
                }
            });
        }
    }
}