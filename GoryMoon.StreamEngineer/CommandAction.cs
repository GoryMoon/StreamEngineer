using System;
using System.Linq;
using GoryMoon.StreamEngineer.Data;
using Newtonsoft.Json.Linq;
using Sandbox;
using Sandbox.Engine.Multiplayer;
using Sandbox.Engine.Platform;
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
            if (args != null && args.Length >= 1)
            {
                if (args[0] == "clear")
                {
                    if (Game.IsDedicated)
                    {
                        MyMultiplayer.RaiseStaticEvent(x => Clear);
                    }
                    else
                    {
                        Clear();
                    }
                }
                else if (args[0] == "action")
                {
                    var type = args.Length > 1 ? args[1] : null;
                    var data = args.Length > 2 ? string.Join(" ", args.Skip(2)) : null;

                    if (Game.IsDedicated)
                    {
                        MyMultiplayer.RaiseStaticEvent(x => Action, type, data, new EndpointId(), new Vector3D?());
                    }
                    else
                    {
                        Action(type, data);
                    }
                }
            }
        }

        [Event(null, 213)]
        [Reliable]
        [Server]
        public static void Clear()
        {
            if (MySession.Static.GetUserPromoteLevel(MyEventContext.Current.Sender.Value) < MyPromoteLevel.Admin)
            {
                MyEventContext.ValidationFailed();
            }
            else
            {
                ActionNotification.Clear();
            }
        }

        [Event(null, 214)]
        [Reliable]
        [Server]
        public static void Action(string action, [Nullable] string data)
        {
            if (MySession.Static.GetUserPromoteLevel(MyEventContext.Current.Sender.Value) < MyPromoteLevel.Admin)
            {
                MyEventContext.ValidationFailed();
            }
            else
            {
                MySandboxGame.Log.WriteLineAndConsole("Executing /se action command: " + action);
                if (action != null)
                {
                    try
                    {
                        Plugin.Static.DataHandler.Execute(action, JToken.Parse(data ?? "{}"),
                            new Data.Data
                            {
                                Type = EventType.TwitchExtension,
                                Amount = 1
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

        public string CommandText => "/se";

        public string HelpText => "Runs an action. Usage: '/se clear' or '/se action <type> <json data>'";
        public string HelpSimpleText => "Runs an action.";
        
        public MyPromoteLevel VisibleTo => MyPromoteLevel.Admin;
    }
}