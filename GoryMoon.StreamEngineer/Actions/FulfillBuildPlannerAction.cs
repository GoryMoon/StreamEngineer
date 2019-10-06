using GoryMoon.StreamEngineer.Data;
using Sandbox.Game.Entities;
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
                    foreach (var planItem in character.BuildPlanner)
                    {
                        ActionHandler.Logger.WriteLine("Planned item: " + planItem.BlockDefinition.Id);
                        foreach (var component in planItem.Components)
                        {
                            var newObject = MyObjectBuilderSerializer.CreateNewObject(component.ComponentDefinition.Id);
                            if (inventory.AddItems(component.Count, newObject))
                            {
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