using System.Collections.Generic;
using GoryMoon.StreamEngineer.Data;

namespace GoryMoon.StreamEngineer.Actions
{
    public class EnableHelmetAction: BaseAction
    {
        public override void Execute(Data.Data data, Dictionary<string, object> parameters)
        {
            SessionHandler.EnqueueAction(() =>
            {
                var player = Utils.GetPlayer();
                if (player == null) return;
                
                var controlledEntity = player.Controller.ControlledEntity;
                var helmet = controlledEntity?.EnabledHelmet;
                if (helmet.HasValue && !helmet.Value)
                {
                    controlledEntity?.SwitchHelmet();
                }
            });
        }
    }
    
    
}