using System;
using System.Collections.Generic;
using System.Linq;
using GoryMoon.StreamEngineer.Data;
using Newtonsoft.Json;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.Game.SessionComponents;
using Sandbox.Game.World;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

namespace GoryMoon.StreamEngineer.Actions
{
    public class SmiteAction: BaseAction
    {
        [JsonIgnore]
        public new static string TypeName => "smite";

        public bool Random { get; set; } = false;
        
        [JsonProperty("hit_player")]
        public bool HitPlayer { get; set; } = true;
        [JsonProperty("hit_grid")]
        public bool HitGrid { get; set; } = true;
        [JsonProperty("do_damage")]
        public bool DoDamage { get; set; } = false;
        public int Damage { get; set; } = -1;
        
        public override void Execute(Data.Data data, Dictionary<string, object> parameters)
        {
            SessionHandler.EnqueueAction(() =>
            {
                var player = Utils.GetPlayer();
                if (player != null)
                {
                    if (MyGamePruningStructure.GetClosestPlanet(player.GetPosition()) == null)
                    {
                        Utils.SendChat("Can't run Smite as the target isn't on a planet.");
                        return;
                    }

                    var weatherComponent = MySession.Static.GetComponent<MySectorWeatherComponent>();
                    var weatherLightning = new MyObjectBuilder_WeatherLightning();
                    var hasWeather = weatherComponent.GetWeather(player.GetPosition(), out var weatherEffect);
                    if (hasWeather)
                    {
                        var weatherEffectDef = MyDefinitionManager.Static.GetWeatherEffect(weatherEffect.Weather);
                        if (weatherEffectDef?.Lightning != null)
                            weatherLightning = (MyObjectBuilder_WeatherLightning)MyObjectBuilderSerializer.Clone(weatherEffectDef.Lightning);
                    }

                    if (Damage > -1)
                        weatherLightning.Damage = Damage;
                    
                    if (Random)
                    {
                        if (hasWeather)
                            weatherComponent.CreateRandomLightning(weatherEffect, weatherLightning, HitGrid, HitPlayer);
                    }
                    else
                        weatherComponent.CreateLightning(player.GetPosition(), weatherLightning, DoDamage);
                }
            });
        }
    }
}