using System.Collections.Generic;
using GoryMoon.StreamEngineer.Data;
using Newtonsoft.Json;
using Sandbox.Game.Entities;

namespace GoryMoon.StreamEngineer.Actions
{
    public class ToggleThrustersAction: BaseAction
    {
        [JsonIgnore]
        public new static string TypeName => "toggle_thrusters";

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
                        block.Enabled = !block.Enabled;
                    }
                }
                else
                {
                    ActionHelper.SetThrusters(ActionHelper.ActionEnum.Toggle, player.Id.SteamId);
                }
            });
        }
    }
    
    
}