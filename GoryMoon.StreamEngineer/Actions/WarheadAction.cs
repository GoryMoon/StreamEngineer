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
                MyCubeBuilder.SpawnDynamicGrid(definitionBase, player.Character, world, color, MyStringHash.NullOrEmpty,
                    0L, MyCubeBuilder.SpawnFlags.Default | MyCubeBuilder.SpawnFlags.SpawnAsMaster,
                    player.Character.EntityId, entity =>
                    {
                        var grid = (MyCubeGrid) entity;

                        var factions = MySession.Static.Factions;
                        foreach (var pair in factions.Factions)
                        {
                            if (factions.IsNpcFaction(pair.Key) && factions.IsFactionWithPlayerEnemy(player.Identity.IdentityId, pair.Key))
                            {
                                grid.ChangeGridOwner(pair.Key, MyOwnershipShareModeEnum.None);
                                break;
                            }
                        }
                        
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
        }
    }
    
}