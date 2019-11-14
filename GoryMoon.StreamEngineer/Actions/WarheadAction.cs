using GoryMoon.StreamEngineer.Data;
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
    public class WarheadAction: BaseAction
    {
        public string Speed { get; set; }
        public string Distance { get; set; }
        public string Countdown { get; set; }

        public override void Execute(Data.Data data)
        {
            SessionHandler.EnqueueAction(() =>
            {
                var player = Utils.GetPlayer();
                if (player != null)
                {
                    var id = new MyDefinitionId((MyObjectBuilderType) typeof(MyObjectBuilder_Warhead), "LargeWarhead");
                    var definitionBase = (MyWarheadDefinition) MyDefinitionManager.Static.GetCubeBlockDefinition(id);

                    var up = player.Character.PositionComp.GetOrientation().Up;
                    up.Normalize();
                    var pos = player.GetPosition() + up * GetEventValue(Distance, 100, data);
                    MatrixD world = MatrixD.CreateWorld(pos, Vector3.Forward, Vector3.Up);
                    Vector3 color = Color.Red.ColorToHSV();
                    var pirateFaction = MySession.Static.Factions.TryGetFactionByTag("SPRT");
                    MyCubeBuilder.SpawnDynamicGrid(definitionBase, player.Character, world, color,
                        MyStringHash.NullOrEmpty,
                        0L, MyCubeBuilder.SpawnFlags.Default | MyCubeBuilder.SpawnFlags.SpawnAsMaster,
                        pirateFaction.FounderId, entity =>
                        {
                            var grid = (MyCubeGrid) entity;
                            foreach (var warhead in grid.GetFatBlocks<MyWarhead>())
                            {
                                warhead.IsArmed = true;
                                warhead.DetonationTime = (float) GetEventValue(Countdown, 10, data);
                                warhead.StartCountdown();
                            }

                            var direction = player.GetPosition() - grid.PositionComp.GetPosition();
                            direction.Normalize();
                            grid.Physics.LinearVelocity += direction * GetEventValue(Speed, 5, data);
                        });
                }
            });
        }
    }
    
}