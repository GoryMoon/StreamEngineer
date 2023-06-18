using System.Collections.Generic;
using GoryMoon.StreamEngineer.Data;
using Newtonsoft.Json;
using Sandbox.Game.World;

namespace GoryMoon.StreamEngineer.Actions
{
    public class CommandAction: BaseAction
    {
        [JsonIgnore]
        public static string TypeName => "command";
        
        [JsonProperty("command")]
        public string Command { get; set; }

        public override void Execute(Data.Data data, Dictionary<string, object> parameters)
        {
            SessionHandler.EnqueueAction(() =>
            {
                if (MySession.Static.ChatSystem.CommandSystem.CanHandle(Command))
                    MySession.Static.ChatSystem.CommandSystem.Handle(Command);
                else
                    Utils.SendChat("No able to run command: " + Command);
            });
        }
    }
}