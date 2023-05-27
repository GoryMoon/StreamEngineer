using System.Collections.Generic;
using GoryMoon.StreamEngineer.Data;
using Newtonsoft.Json;

namespace GoryMoon.StreamEngineer.Actions
{
    public class TogglePowerAction: BaseAction
    {
        [JsonIgnore]
        public new static string TypeName => "toggle_power";
        
        public override void Execute(Data.Data data, Dictionary<string, object> parameters)
        {
            SessionHandler.EnqueueAction(() =>
            {
                var player = Utils.GetPlayer();
                if (player == null) return;
                ActionHelper.SetPower(ActionHelper.ActionEnum.Toggle, player.Id.SteamId);
            });
        }
    }
    
    
}