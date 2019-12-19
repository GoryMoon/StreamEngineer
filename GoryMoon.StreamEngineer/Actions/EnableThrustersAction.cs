using System.Collections.Generic;
using GoryMoon.StreamEngineer.Data;
using Sandbox.Game.Entities;

namespace GoryMoon.StreamEngineer.Actions
{
    public class EnableThrustersAction: BaseAction
    {
        public override void Execute(Data.Data data, Dictionary<string, object> parameters)
        {
            SessionHandler.EnqueueAction(() =>
            {
                var player = Utils.GetPlayer();
                if (player == null) return;
                var controlledEntity = player.Controller.ControlledEntity;

                if (controlledEntity is MyShipController controller)
                {
                    var blocks = controller.CubeGrid.GetFatBlocks<MyThrust>();
                    foreach (var block in blocks)
                    {
                        block.Enabled = true;
                    }
                }
                else
                {
                    ActionHelper.SetThrusters(ActionHelper.ActionEnum.Enable, player.Id.SteamId);
                }
            });
        }
    }
    
    
}