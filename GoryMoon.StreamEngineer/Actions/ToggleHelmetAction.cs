using System.Collections.Generic;
using GoryMoon.StreamEngineer.Data;

namespace GoryMoon.StreamEngineer.Actions
{
    public class ToggleHelmetAction: BaseAction
    {
        public override void Execute(Data.Data data, Dictionary<string, object> parameters)
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