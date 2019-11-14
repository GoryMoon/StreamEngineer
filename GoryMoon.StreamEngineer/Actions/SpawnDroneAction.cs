using System;
using System.Linq;
using GoryMoon.StreamEngineer.Config;
using GoryMoon.StreamEngineer.Data;
using Harmony;
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
    public class SpawnDroneAction: BaseAction
    {
        private readonly FastInvokeHandler _spawnDrone;

        public string Drone { get; set; } = null;
        
        private readonly Random _random = new Random();

        public SpawnDroneAction()
        {
            _spawnDrone = MethodInvoker.GetHandler(AccessTools.Method(typeof(MyPirateAntennas), "SpawnDrone", new []{typeof(MyRadioAntenna), typeof(long), typeof(Vector3D), typeof(MySpawnGroupDefinition), typeof(Vector3), typeof(Vector3)}));
        }

        public override void Execute(Data.Data data)
        {
            SessionHandler.EnqueueAction(() =>
            {
                var player = Utils.GetPlayer();
                if (player != null)
                {
                    var id = new MyDefinitionId((MyObjectBuilderType) typeof(MyObjectBuilder_RadioAntenna), "SmallBlockRadioAntenna");
                    var up = player.Character.PositionComp.GetOrientation().Up;
                    up.Normalize();
                    var definitionBase = (MyRadioAntennaDefinition) MyDefinitionManager.Static.GetCubeBlockDefinition(id);
                    var pos = player.GetPosition() + up * 50;
                    MatrixD world = MatrixD.CreateWorld(pos, Vector3.Forward, Vector3.Up);
                    Vector3 color = Color.Red.ColorToHSV();
                    var pirateFaction = MySession.Static.Factions.TryGetFactionByTag("SPRT");
                    MyCubeBuilder.SpawnDynamicGrid(definitionBase, player.Character, world, color,
                        MyStringHash.NullOrEmpty,
                        0L, MyCubeBuilder.SpawnFlags.Default | MyCubeBuilder.SpawnFlags.SpawnAsMaster,
                        pirateFaction.FounderId, entity =>
                        {
                            var grid = (MyCubeGrid) entity;
                            var pirateAntennas = MySession.Static.GetComponent<MyPirateAntennas>();
                            if (pirateAntennas != null)
                            {
                                var drones = Utils.IsInSpace(pos) ? Configuration.Plugin.Get(c => c.SpaceDrones): Configuration.Plugin.Get(c => c.PlanetDrones);
                                var drone = (Drone ?? drones[_random.Next(drones.Length)]) ?? "Vulture Drone";
                                
                                MyDefinitionManager.Static.TryGetDefinition(
                                    new MyDefinitionId(
                                        (MyObjectBuilderType) typeof(MyObjectBuilder_SpawnGroupDefinition),
                                        drone), out MySpawnGroupDefinition spawnGroup);
                                if (spawnGroup != null)
                                {
                                    Vector3D? nullable = new Vector3D?();
                                    for (int index = 0; index < 10; ++index)
                                    {
                                        world = entity.WorldMatrix;
                                        nullable = MyEntities.FindFreePlace(
                                            world.Translation + MyUtils.GetRandomVector3Normalized() * 1500,
                                            spawnGroup.SpawnRadius);
                                        if (nullable.HasValue)
                                            break;
                                    }

                                    foreach (var antenna in grid.GetFatBlocks<MyRadioAntenna>())
                                    {
                                        _spawnDrone.Invoke(pirateAntennas,
                                            new object[]
                                            {
                                                antenna, pirateFaction.FounderId, nullable.Value, spawnGroup,
                                                new Vector3?(), new Vector3?()
                                            });
                                    }
                                    
                                    var spawns = spawnGroup.Prefabs.Select(prefab => prefab.SubtypeId.ToSplitCase())
                                        .GroupBy(s => s).Select(grouping =>
                                            (grouping.Count() > 1 ? grouping.Count() + "x " : "") + grouping.Key)
                                        .Join();
                                    var msg = "Spawned " + spawns;
                                    Utils.SendChat(msg);
                                    ActionNotification.SendActionMessage(msg, Color.Red, "ArcHudGPSNotification1");
                                }
                            }
                            grid.CubeBlocks.ToList().ForEach(block => grid.RemoveBlock(block));
                        });
                }
            });
        }
    }
}