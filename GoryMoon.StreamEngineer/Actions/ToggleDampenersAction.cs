using GoryMoon.StreamEngineer.Data;
using Sandbox.Game.Entities;

namespace GoryMoon.StreamEngineer.Actions
{
    public class ToggleDampenersAction: BaseAction
    {
        public override void Execute(Data.Data data)
        {
            SessionHandler.EnqueueAction(() =>
            {
                var player = Utils.GetPlayer();
                player?.Controller.ControlledEntity.SwitchDamping();
            });
        }
    }
    
    
}