using GoryMoon.StreamEngineer.Data;
using Sandbox.Game.Entities;
using VRage.Game;
using VRage.ObjectBuilders;
using VRageMath;

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
                    foreach (var planItem in character.BuildPlanner)
                    {
                        foreach (var component in planItem.Components)
                        {
                            var newObject = MyObjectBuilderSerializer.CreateNewObject(component.ComponentDefinition.Id);
                            if (inventory.AddItems(component.Count, newObject))
                            {
                                component.Count = 0;
                            }
                            else
                            {
                                var matrix = player.Character.WorldMatrix;
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