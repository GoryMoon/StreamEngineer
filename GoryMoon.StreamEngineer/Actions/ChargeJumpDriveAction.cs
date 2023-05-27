using System.Collections.Generic;
using GoryMoon.StreamEngineer.Data;
using Newtonsoft.Json;
using Sandbox.Game.Entities;

namespace GoryMoon.StreamEngineer.Actions
{
    public class ChargeJumpDriveAction: BaseAction
    {
        [JsonIgnore]
        public new static string TypeName => "charge_jump_drive";

        public override void Execute(Data.Data data, Dictionary<string, object> parameters)
        {
            SessionHandler.EnqueueAction(() =>
            {
                var player = Utils.GetPlayer();
                if (player != null)
                {
                    var controlledEntity = player.Controller.ControlledEntity;

                    if (controlledEntity is MyShipController controller)
                    {
                        var blocks = controller.CubeGrid.GetFatBlocks<MyJumpDrive>();
                        foreach (var block in blocks)
                        {
                            block.CurrentStoredPower = block.BlockDefinition.PowerNeededForJump;
                        }
                    }
                }
            });
        }
    }
}