using System.Collections.Generic;
using GoryMoon.StreamEngineer.Data;
using Newtonsoft.Json;
using Sandbox.Definitions;
using VRage.Game;

namespace GoryMoon.StreamEngineer.Actions
{
    public class GiveItemAction: BaseAction
    {
        [JsonIgnore]
        public new static string TypeName => "give_item";

        public Item[] Items { get; set; }
        
        public override void Execute(Data.Data data, Dictionary<string, object> parameters)
        {
            SessionHandler.EnqueueAction(() =>
            {
                var player = Utils.GetPlayer();
                if (player != null && Items != null)
                {
                    var character = player.Character;
                    var worldMatrix = character.WorldMatrix;
                    foreach (var item in Items)
                    {
                        if (MyDefinitionId.TryParse("MyObjectBuilder_" + item.Id, out var id) &&
                            MyDefinitionManager.Static.TryGetPhysicalItemDefinition(id, out var itemDefinition))
                        {
                            var amount = GetEventValue(item.Amount, 1, parameters);
                            Utils.AddOrDropItem(player, itemDefinition, ref amount, worldMatrix);
                        }
                    }
                }
            });
        }
    }

    public class Item
    {
        public string Id { get; set; }
        public string Amount { get; set; }
    }
}