using System.Collections.Generic;
using GoryMoon.StreamEngineer.Data;
using Sandbox.Game.Entities;

namespace GoryMoon.StreamEngineer.Actions
{
    public class TogglePowerAction: BaseAction
    {
        public override void Execute(Data.Data data, Dictionary<string, object> parameters)
        {
            SessionHandler.EnqueueAction(() =>
            {
                var player = Utils.GetPlayer();
                if (player != null)
                {
                    var isUsing = player.Character.IsUsing;
                    if (isUsing is MyShipController shipController )
                    {
                        shipController.SwitchReactors();
                    }
                }
            });
        }
    }
    
    
}