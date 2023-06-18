using System.Collections.Generic;
using System.Linq;
using GoryMoon.StreamEngineer.Data;
using Newtonsoft.Json;
using Sandbox.Definitions;
using Sandbox.Game.SessionComponents;
using Sandbox.Game.World;
using VRage.Utils;
using VRageMath;

namespace GoryMoon.StreamEngineer.Actions
{
    public class ChangeWeatherAction: BaseAction
    {
        [JsonIgnore]
        public new static string TypeName => "change_weather";

        public string[] Weather { get; set; }

        public float Radius { get; set; } = 0;
        public int Length { get; set; } = 0;
        
        public override void Execute(Data.Data data, Dictionary<string, object> parameters)
        {
            SessionHandler.EnqueueAction(() =>
            {
                var player = Utils.GetPlayer();
                if (player != null)
                {
                    var weatherDefinitions = MyDefinitionManager.Static.GetWeatherDefinitions();

                    if (Weather == null || Weather.Length <= 0)
                        Weather = weatherDefinitions.Select(def => def.Id.SubtypeName).Append("Clear").ToArray();

                    var filterList = weatherDefinitions
                        .Where(def => def.Public)
                        .Select(def => def.Id.SubtypeName)
                        .Append("Clear").Append("clear");
                    var validList = Weather.Where(s => filterList.Contains(s)).ToArray();

                    if (validList.Length > 0)
                    {
                        var weather = validList.GetRandomItem();
                        var weatherComponent = MySession.Static.GetComponent<MySectorWeatherComponent>();
                        weatherComponent.SetWeather(weather, Radius, player.GetPosition(), false, Vector3D.Zero, Length);

                        var displayName = weatherDefinitions.FirstOrDefault(def => def.Id.SubtypeName == weather)?.DisplayNameText ?? weather;
                        Utils.SendChat("Changed weather to: " + displayName);
                    }
                }
            });
        }
    }
}