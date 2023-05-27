using System.Collections.Generic;
using GoryMoon.StreamEngineer.Data;
using Newtonsoft.Json;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.World;
using VRage.Game;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace GoryMoon.StreamEngineer.Actions
{
    public class WarheadAction : BaseAction
    {
        [JsonIgnore]
        public new static string TypeName => "warhead";

        [JsonProperty("hostile")]
        public bool Hostile { get; set; } = false;
        
        [JsonProperty("space_speed")]
        public string SpaceSpeed { get; set; }
        [JsonProperty("space_distance")]
        public string SpaceDistance { get; set; }
        [JsonProperty("space_countdown")]
        public string SpaceCountdown { get; set; }
        
        [JsonProperty("speed")]
        public string Speed { get; set; }
        [JsonProperty("distance")]
        public string Distance { get; set; }
        [JsonProperty("countdown")]
        public string Countdown { get; set; }

        public override void Execute(Data.Data data, Dictionary<string, object> parameters)
        {
            SessionHandler.EnqueueAction(() =>
            {
                var player = Utils.GetPlayer();
                if (player != null)
                {
                    var id = new MyDefinitionId((MyObjectBuilderType) typeof(MyObjectBuilder_Warhead), "LargeWarhead");
                    var definitionBase = (MyWarheadDefinition) MyDefinitionManager.Static.GetCubeBlockDefinition(id);

                    var inSpace = Utils.IsInSpace(player.GetPosition());
                    var up = player.Character.PositionComp.GetOrientation().Up;
                    up.Normalize();
                    var pos = player.GetPosition() + up * GetEventValue(inSpace && SpaceDistance != null ? SpaceDistance: Distance, 100, parameters);
                    var world = MatrixD.CreateWorld(pos, Vector3.Forward, Vector3.Up);
                    var color = Color.Red.ColorToHSV();
                    var pirateFaction = MySession.Static.Factions.TryGetFactionByTag("SPRT");
                    MyCubeBuilder.SpawnDynamicGrid(definitionBase, player.Character, world, color,
                        MyStringHash.NullOrEmpty,
                        0L, MyCubeBuilder.SpawnFlags.Default | MyCubeBuilder.SpawnFlags.SpawnAsMaster,
                        Hostile ? pirateFaction.FounderId: player.Character.EntityId, entity =>
                        {
                            var grid = (MyCubeGrid) entity;
                            foreach (var warhead in grid.GetFatBlocks<MyWarhead>())
                            {
                                warhead.IsArmed = true;
                                warhead.DetonationTime = (float) GetEventValue(inSpace && SpaceCountdown != null ? SpaceCountdown: Countdown, 10, parameters);
                                warhead.StartCountdown();
                            }

                            var direction = player.GetPosition() - grid.PositionComp.GetPosition();
                            direction.Normalize();
                            grid.Physics.LinearVelocity += direction * GetEventValue(inSpace && SpaceSpeed != null ? SpaceSpeed: Speed, 5, parameters);
                        });
                }
            });
        }
    }
    
}