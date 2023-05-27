using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GoryMoon.StreamEngineer.Config;
using GoryMoon.StreamEngineer.Data;
using Newtonsoft.Json.Linq;

namespace GoryMoon.StreamEngineer.Actions
{
    public partial class DataHandler
    {
        public override void OnIntegrationApp(JObject action)
        {
            var type = action.Value<string>("type") ?? string.Empty;
            var settings = action.GetValue("values");
            
            if (settings != null && type == GiveItemAction.TypeName)
                settings["items"] = new JArray(settings.Value<JArray>("items")
                    .Select(token => JObject.Parse(token.Value<string>())));

            if (_actionHandler.GetAction(type, settings, out var baseAction))
            {
                var metadata = action.Value<JObject>("metadata");
                var name = metadata?.Value<string>("user") ?? "";
                
                var data = new Data.Data { Type = EventType.IntegrationApp, Amount = -1 };
                baseAction.Execute(data, GetParams(data));

                var textInfo = new CultureInfo("en-US", false).TextInfo;

                var messageEvent = Configuration.Plugin.Get(c => c.Events.IntegrationAppAction);
                SendMessage(string.Format(messageEvent.Message, name, textInfo.ToTitleCase(type).ToSplitCase()), messageEvent.AlwaysSendMessage,
                    new List<BaseAction> { baseAction });
            }
        }
    }
}