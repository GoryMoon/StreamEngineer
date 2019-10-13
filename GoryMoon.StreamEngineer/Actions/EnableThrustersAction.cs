using GoryMoon.StreamEngineer.Data;
using Sandbox.Game.Entities;

namespace GoryMoon.StreamEngineer.Actions
{
    public class EnableThrustersAction: BaseAction
    {
        public override void Execute(Data.Data data)
        {
            SessionHandler.EnqueueAction(() =>
            {
                var player = Utils.GetPlayer();
                if (player != null)
                {
                    var controlledEntity = player.Controller.ControlledEntity;

                    if (!controlledEntity.EnabledThrusts)
                    {
                        controlledEntity.SwitchThrusts();
                    }
                }
            });
        }
    }
    
    
}