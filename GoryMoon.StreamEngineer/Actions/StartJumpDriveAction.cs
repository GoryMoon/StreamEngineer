using System.Collections.Generic;
using GoryMoon.StreamEngineer.Data;
using Newtonsoft.Json;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;

namespace GoryMoon.StreamEngineer.Actions
{
    //TODO: WIP figure this out, runs on server but asks for player input, want to start jump directly if possible
    public class StartJumpDriveAction: BaseAction
    {
        [JsonIgnore]
        public new static string TypeName => "start_jump_drive";

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
                        var grid = controller.CubeGrid;
                        var blocks = grid.GetFatBlocks<MyJumpDrive>();
                        if (blocks.MoveNext())
                        {
                            var jumpDrive = blocks.Current;
                            if (jumpDrive != null && jumpDrive.CanJump && !grid.GridSystems.JumpSystem.IsJumping)
                            {
                                ((IMyJumpDrive) jumpDrive).Jump(false);
                            }
                        }
                    }
                }
            });
        }
    }
}