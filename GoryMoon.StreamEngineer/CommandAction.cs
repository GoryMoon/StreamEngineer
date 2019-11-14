using System;
using System.Linq;
using GoryMoon.StreamEngineer.Data;
using Newtonsoft.Json.Linq;
using Sandbox;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.GameSystems.Chat;
using Sandbox.Game.World;
using VRage.Game.ModAPI;
using VRage.Network;
using VRage.Serialization;
using VRageMath;

namespace GoryMoon.StreamEngineer
{
    [StaticEventOwner]
    public class CommandAction: IMyChatCommand
    {
        public void Handle(string[] args)
        {
            if (args != null)
            {
                var type = args.Length > 0 ? args[0] : null;
                var amount = args.Length > 1 ? args[1] : null;
                var data = args.Length > 2 ? string.Join(" ", args.Skip(2)):null;
                
                MyMultiplayer.RaiseStaticEvent(x => Action, type, data, amount, new EndpointId(), new Vector3D?());
            }
        }

        [Event(null, 214)]
        [Reliable]
        [Server]
        public static void Action(string action, [Nullable] string data, [Nullable] string amount)
        {
            if (MySession.Static.GetUserPromoteLevel(MyEventContext.Current.Sender.Value) < MyPromoteLevel.Admin)
            {
                MyEventContext.ValidationFailed();
            }
            else
            {
                MySandboxGame.Log.WriteLineAndConsole("Executing /action command");
                if (action != null)
                {
                    try
                    {
                        Plugin.Static.DataHandler.Execute(action, JToken.Parse(data ?? "{}"),
                            new Data.Data
                            {
                                Type = EventType.TwitchExtension,
                                Amount = amount != null ? int.Parse(amount) : 100
                            });
                    }
                    catch (Exception e)
                    {
                        Utils.SendChat(e.Message);
                        Plugin.Static.Logger.WriteLine(e);
                    }
                }
            }
        }

        public string CommandText => "/action";

        public string HelpText => "Runs an action. Usage: '/action <type> <amount> <json data>'";
        public string HelpSimpleText => "Runs an action.";
        
        public MyPromoteLevel VisibleTo => MyPromoteLevel.Admin;
    }
}