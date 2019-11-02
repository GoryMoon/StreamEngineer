using System;
using GoryMoon.StreamEngineer.Config;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using VRage;
using VRage.Game;
using VRage.ObjectBuilders;
using VRageMath;

namespace GoryMoon.StreamEngineer
{
    public static class Utils
    {

        public static MyPlayer GetPlayer()
        {
            return MySession.Static.Players.GetPlayerByName(Configuration.Plugin.Get(c => c.SteamName));
        }

        public static void AddOrDropItem(MyPlayer player, MyPhysicalItemDefinition item, ref double amount, MatrixD position)
        {
            var inventory = player.Character.GetInventory();
            var newObject = MyObjectBuilderSerializer.CreateNewObject(item.Id);

            amount = TryAddToInventory(inventory, amount, newObject, item.Id);
            if (amount > 0)
            {
                var controlledEntity = player.Controller.ControlledEntity;
                if (controlledEntity is MyShipController controller)
                {
                    foreach (var gridInventory in controller.CubeGrid.Inventories)
                    {
                        amount = TryAddToInventory(gridInventory.GetInventory(), amount, newObject, item.Id);
                        if (amount <= Double.Epsilon)
                        {
                            break;
                        }
                    }
                }

                if (amount > 0)
                {
                    MyFloatingObjects.Spawn(item, position.Translation, position.Forward, position.Up, (int) Math.Ceiling(amount));
                    amount = 0;
                }
            }
        }

        private static double TryAddToInventory(MyInventory inventory, double amount, MyObjectBuilder_Base newObject, MyDefinitionId itemId)
        {
            var remaining = 0D;
            if (amount > 1)
            {
                for (var i = 0; i < amount; i++)
                {
                    if (!inventory.CanItemsBeAdded(1, itemId) || !inventory.AddItems(1, newObject))
                    {
                        remaining = amount - i;
                        break;
                    }
                }
            }
            else
            {
                if (!inventory.CanItemsBeAdded(1, itemId) || !inventory.AddItems((MyFixedPoint) amount, newObject))
                {
                    remaining = amount;
                }
            }

            return remaining;
        }
    }
}