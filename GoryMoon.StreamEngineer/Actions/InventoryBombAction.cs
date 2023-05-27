using System.Collections.Generic;
using GoryMoon.StreamEngineer.Data;
using Newtonsoft.Json;
using Sandbox.Game.Entities;

namespace GoryMoon.StreamEngineer.Actions
{
    public class InventoryBombAction: BaseAction
    {
        [JsonIgnore]
        public new static string TypeName => "inventory_bomb";

        public override void Execute(Data.Data data, Dictionary<string, object> parameters)
        {
            SessionHandler.EnqueueAction(() =>
            {
                var player = Utils.GetPlayer();
                if (player != null)
                {
                    var character = player.Character;
                    
                    var inventory = player.Character.GetInventory();
                    foreach (var physicalInventoryItem in inventory.GetItems())
                    {
                        MyFloatingObjects.EnqueueInventoryItemSpawn(physicalInventoryItem, character.PositionComp.WorldAABB, character.Physics.GetVelocityAtPoint(player.GetPosition()));
                    }
                    inventory.Clear();
                }
            });
        }
    }
}