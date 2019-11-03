using GoryMoon.StreamEngineer.Data;

namespace GoryMoon.StreamEngineer.Actions
{
    public class ToggleHelmetAction: BaseAction
    {
        public override void Execute(Data.Data data)
        {
            SessionHandler.EnqueueAction(() =>
            {
                var player = Utils.GetPlayer();
                var controlledEntity = player?.Controller.ControlledEntity;
                controlledEntity?.SwitchHelmet();
            });
        }
    }
    
    
}