using GoryMoon.StreamEngineer.Data;
using Sandbox.Game.Entities;

namespace GoryMoon.StreamEngineer.Actions
{
    public class DisableThrustersAction: BaseAction
    {
        public override void Execute(Data.Data data)
        {
            SessionHandler.EnqueueAction(() =>
            {
                var player = Utils.GetPlayer();
                if (player == null) return;
                var controlledEntity = player?.Controller.ControlledEntity;

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
                    var thrusts = controlledEntity?.EnabledThrusts;
                    if (thrusts.HasValue && thrusts.Value)
                    {
                        controlledEntity?.SwitchThrusts();
                    }
                }
            });
        }
    }
    
    
}