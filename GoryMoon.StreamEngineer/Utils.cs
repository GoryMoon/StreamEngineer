using System;
using System.Linq;
using GoryMoon.StreamEngineer.Config;
using Sandbox;
using Sandbox.Definitions;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Gui;
using Sandbox.Game.World;
using VRage;
using VRage.Game;
using VRage.ObjectBuilders;
using VRageMath;
using Game = Sandbox.Engine.Platform.Game;

namespace GoryMoon.StreamEngineer
{
    public static class Utils
    {

        public static MyPlayer GetPlayer()
        {
            if (Game.IsDedicated)
                return MySession.Static.Players.GetPlayerByName(Configuration.Plugin.Get(c => c.SteamName));

            return MySession.Static.LocalHumanPlayer;
        }

        public static bool IsInSpace(Vector3D pos)
        {
            var closestPlanet = MyGamePruningStructure.GetClosestPlanet(pos);
            return closestPlanet == null || !closestPlanet.HasAtmosphere ||
                   closestPlanet.GetAirDensity(pos) <= 0.5;
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

        public static void SendChat(string msg, bool fromQueue = false)
        {
            if (!MySandboxGame.IsGameReady)
            {
                if (!fromQueue)
                    Plugin.EnqueueMessage(msg, true);
                return;
            }
            if (MyMultiplayer.Static != null)
                MyMultiplayer.Static.SendChatMessage(msg, ChatChannel.GlobalScripted, 0,
                "[StreamEngineer]");
            else
                MyHud.Chat.ShowMessageScripted("[StreamEngineer]", msg);
        }
        
        public static string ToSplitCase(this string inputString)
        {
            return inputString.Aggregate(string.Empty, (result, next) =>
            {
                if ((char.IsUpper(next) || next == '_') && result.Length > 0) 
                    result += ' ';

                if (next != '_')
                    result += next;

                return result;
            });
        }
    }
}