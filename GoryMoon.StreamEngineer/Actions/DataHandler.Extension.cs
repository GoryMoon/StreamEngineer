using System.Collections.Generic;
using GoryMoon.StreamEngineer.Config;
using GoryMoon.StreamEngineer.Data;
using Newtonsoft.Json.Linq;

namespace GoryMoon.StreamEngineer.Actions
{
    public partial class DataHandler
    {
        public override void OnTwitchExtension(string name, int amount, string action, JToken settings)
        {
            if (_actionHandler.GetAction(action, settings, out var baseAction))
            {
                var data = new Data.Data { Type = EventType.TwitchExtension, Amount = amount };
                baseAction.Execute(data, GetParams(data));
                
                var messageEvent = Configuration.Plugin.Get(c => c.Events.TwitchExtension);
                if (baseAction.Message != null && baseAction.Message.Trim().Length > 0)
                    SendMessage(string.Format(messageEvent.WithMessage, name, amount), true,
                        new List<BaseAction> { baseAction });
                else
                    SendMessage(string.Format(messageEvent.WithoutMessage, name, amount), true,
                        new List<BaseAction> { baseAction });
            }
        }

        public override void OnTwitchChannelPoints(string name, string id)
        {
            var actions = GetAndExecute(new Data.Data { Type = EventType.TwitchChannelPoints, Id = id });

            var messageEvent = Configuration.Plugin.Get(c => c.Events.TwitchChannelPoints);
            SendMessage(string.Format(messageEvent.Message, name), messageEvent.AlwaysSendMessage, actions);
        }
    }
}