using GoryMoon.StreamEngineer.Data;

namespace GoryMoon.StreamEngineer.Actions
{
    public class DisableHelmetAction: BaseAction
    {
        public override void Execute(Data.Data data)
        {
            SessionHandler.EnqueueAction(() =>
            {
                var player = Utils.GetPlayer();
                if (player == null) return;
                
                var controlledEntity = player.Controller.ControlledEntity;
                var helmet = controlledEntity?.EnabledHelmet;
                if (helmet.HasValue && helmet.Value)
                {
                    controlledEntity?.SwitchHelmet();
                }
            });
        }
    }
    
    
}