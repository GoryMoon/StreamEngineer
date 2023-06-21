using System;
using System.Collections.Generic;
using System.Linq;
using GoryMoon.StreamEngineer.Config;
using GoryMoon.StreamEngineer.Data;
using HarmonyLib;
using Newtonsoft.Json;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.World;
using VRage.Game;
using VRage.Utils;
using VRageMath;

namespace GoryMoon.StreamEngineer.Actions
{
    public class SpawnDroneAction : BaseAction
    {
        [JsonIgnore]
        public new static string TypeName => "spawn_drone";

        private readonly FastInvokeHandler _spawnDrone;

        public string Drone { get; set; } = null;

        private readonly Random _random = new Random();

        public SpawnDroneAction()
        {
            _spawnDrone = MethodInvoker.GetHandler(AccessTools.Method(typeof(MyPirateAntennas), "SpawnDrone", new[]
            {
                typeof(MyRadioAntenna), typeof(long), typeof(Vector3D), typeof(MySpawnGroupDefinition),
                typeof(Vector3), typeof(Vector3)
            }));
        }

        private static string GetNameFromPrefab(MySpawnGroupDefinition.SpawnGroupPrefab prefab)
        {
            var definition = MyDefinitionManager.Static.GetPrefabDefinition(prefab.SubtypeId);
            if (definition != null)
            {
                if (!string.IsNullOrEmpty(definition.DisplayNameText))
                    return definition.DisplayNameText;

                if (definition.CubeGrids != null && definition.CubeGrids.Length > 0)
                {
                    var grid = definition.CubeGrids[0];
                    if (!string.IsNullOrEmpty(grid.DisplayName))
                        return grid.DisplayName;
                }
            }

            return prefab.SubtypeId.ToSplitCase();
        }

        public override void Execute(Data.Data data, Dictionary<string, object> parameters)
        {
            SessionHandler.EnqueueAction(() =>
            {
                var player = Utils.GetPlayer();
                if (player == null) return;

                var id = new MyDefinitionId(typeof(MyObjectBuilder_RadioAntenna), "SmallBlockRadioAntenna");
                var up = player.Character.PositionComp.GetOrientation().Up;
                up.Normalize();
                var definitionBase =
                    (MyRadioAntennaDefinition)MyDefinitionManager.Static.GetCubeBlockDefinition(id);
                
                var pos = MyEntities.FindFreePlace(
                              player.GetPosition() + (up * 50) + MyUtils.GetRandomVector3Normalized() * 1500, 1) ??
                          player.GetPosition() + up * 50;
                var world = MatrixD.CreateWorld(pos, Vector3.Forward, Vector3.Up);
                var color = Color.Red.ColorToHSV();
                var pirateFaction = MySession.Static.Factions.TryGetFactionByTag("SPRT");
                MyCubeBuilder.SpawnDynamicGrid(definitionBase, player.Character, world, color,
                    MyStringHash.NullOrEmpty,
                    0L, MyCubeBuilder.SpawnFlags.Default | MyCubeBuilder.SpawnFlags.SpawnAsMaster,
                    pirateFaction.FounderId, entity =>
                    {
                        var grid = (MyCubeGrid)entity;
                        var pirateAntennas = MySession.Static.GetComponent<MyPirateAntennas>();
                        if (pirateAntennas != null)
                        {
                            var drones = Utils.IsInSpace(pos)
                                ? Configuration.Plugin.Get(c => c.SpaceDrones)
                                : Configuration.Plugin.Get(c => c.PlanetDrones);
                            var drone =
                                (string.IsNullOrEmpty(Drone) ? drones[_random.Next(drones.Length)] : Drone) ??
                                "Vulture Drone";

                            Plugin.Static.Logger.WriteLine($"Trying to spawn drone with id: {drone}");
                            MyDefinitionManager.Static.TryGetDefinition(
                                new MyDefinitionId(typeof(MyObjectBuilder_SpawnGroupDefinition), drone),
                                out MySpawnGroupDefinition spawnGroup);
                            if (spawnGroup != null)
                            {
                                var location = new Vector3D?();
                                for (var index = 0; index < 10; ++index)
                                {
                                    world = entity.WorldMatrix;
                                    location = MyEntities.FindFreePlace(
                                        world.Translation + MyUtils.GetRandomVector3Normalized() * 1500,
                                        spawnGroup.SpawnRadius);
                                    if (location.HasValue)
                                        break;
                                }

                                foreach (var antenna in grid.GetFatBlocks<MyRadioAntenna>())
                                    _spawnDrone.Invoke(pirateAntennas, antenna, pirateFaction.FounderId,
                                        location.Value, spawnGroup, null, null);

                                var spawns = spawnGroup.Prefabs
                                    .Where(prefab => prefab.SubtypeId != null)
                                    .Select(GetNameFromPrefab)
                                    .GroupBy(s => s)
                                    .Select(grouping =>
                                        (grouping.Count() > 1 ? grouping.Count() + "x " : "") + grouping.Key)
                                    .Join();
                                var msg = "Spawned " + spawns;
                                Utils.SendChat(msg);
                                ActionNotification.SendActionMessage(TypeName, msg, Color.Red);
                            }
                        }

                        grid.CubeBlocks.ToList().ForEach(block => grid.RemoveBlock(block));
                    });
            });
        }
    }
}