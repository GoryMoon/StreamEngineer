using System.Collections.Generic;
using GoryMoon.StreamEngineer.Data;

namespace GoryMoon.StreamEngineer.Actions
{
    public class EnablePowerAction: BaseAction
    {
        public override void Execute(Data.Data data, Dictionary<string, object> parameters)
        {
            SessionHandler.EnqueueAction(() =>
            {
                var player = Utils.GetPlayer();
                if (player == null) return;
                ActionHelper.SetPower.Invoke(ActionHelper.ActionEnum.Enable, player.Id.SteamId);
            });
        }
    }
    
    
}