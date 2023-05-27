using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GoryMoon.StreamEngineer.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TestConsole;

[JsonObject(MemberSerialization.OptIn)]
internal class RandomAction : BaseAction
{
    [JsonIgnore]
    public new static string TypeName => "random";
    
    private DynamicRandomSelector<BaseAction> _actions;

#pragma warning disable 0649
    [JsonExtensionData]
    private IDictionary<string, JToken> _additionalData;
#pragma warning restore 0649
    
    private Random _random = new();

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        if (context.Context is not ActionHandler handler) return;
        
        var actions = _additionalData["actions"].ToObject<List<JToken>>();
        _actions = new DynamicRandomSelector<BaseAction>();
        Message = "No action to randomize from";
        foreach (var action in actions)
            if (handler.GetAction((string)action["type"], action["action"], out var baseAction))
            {
                var weight = (float)(action["weight"] ?? 1.0F);
                _actions.Add(baseAction, weight);
            }

        if (_actions.Count > 0)
            _actions.Build();
    }

    public override void Execute(Data data, Dictionary<string, object> parameters)
    {
        if (_actions.Count <= 0) return;

        var action = _actions.SelectRandomItem();
        Message = action.Message;
        action.Execute(data, parameters);
    }
}