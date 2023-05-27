using System.Collections.Generic;
using GoryMoon.StreamEngineer.Data;
using Newtonsoft.Json;
using Sandbox.Game.Entities;

namespace GoryMoon.StreamEngineer.Actions
{
    public class DisableThrustersAction: BaseAction
    {
        [JsonIgnore]
        public new static string TypeName => "disable_thrusters";
        
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
                        block.Enabled = false;
                    }
                }
                else
                {
                    ActionHelper.SetThrusters(ActionHelper.ActionEnum.Disable, player.Id.SteamId);
                }
            });
        }
    }
    
    
}