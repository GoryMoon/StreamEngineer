using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GoryMoon.StreamEngineer.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GoryMoon.StreamEngineer.Actions
{
    [JsonObject(MemberSerialization.OptIn)]
    public class MultiActions : BaseAction
    {
        private List<BaseAction> _actions;

        [JsonExtensionData] private IDictionary<string, JToken> _additionalData;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (!(context.Context is ActionHandler handler)) return;
            var actions = _additionalData["actions"].ToObject<List<JToken>>();
            _actions = new List<BaseAction>();
            Message = "No actions to run";
            foreach (var action in actions)
            {
                if (handler.GetAction((string) action["type"], action["action"], out var baseAction))
                {
                    _actions.Add(baseAction);
                }
            }
        }

        public override void Execute(Data.Data data, Dictionary<string, object> parameters)
        {
            if (_actions.Count <= 0) return;

            Message = string.Join(" ", _actions.Select(action => action.Message ?? ""));
            _actions.ForEach(action => action.Execute(data, parameters));
        }
    }
}