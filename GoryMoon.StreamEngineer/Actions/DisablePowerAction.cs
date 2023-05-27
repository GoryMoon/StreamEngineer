using System.Collections.Generic;
using GoryMoon.StreamEngineer.Data;
using Newtonsoft.Json;

namespace GoryMoon.StreamEngineer.Actions
{
    public class DisablePowerAction: BaseAction
    {
        [JsonIgnore]
        public new static string TypeName => "disable_power";
        
        public override void Execute(Data.Data data, Dictionary<string, object> parameters)
        {
            SessionHandler.EnqueueAction(() =>
            {
                var player = Utils.GetPlayer();
                if (player == null) return;
                ActionHelper.SetPower(ActionHelper.ActionEnum.Disable, player.Id.SteamId);
            });
        }
    }
}