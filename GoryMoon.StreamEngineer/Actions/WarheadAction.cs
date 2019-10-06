using GoryMoon.StreamEngineer.Data;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using VRage.Game;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace GoryMoon.StreamEngineer.Actions
{
    //TODO WIP NOT WORKING
    public class WarheadAction: BaseAction
    {
        public override void Execute(Data.Data data)
        {
            

            
            var player = Utils.GetPlayer();
            if (player != null)
            {
                var id = new MyDefinitionId((MyObjectBuilderType) typeof(MyObjectBuilder_Warhead), "LargeWarhead");
                var definitionBase = (MyWarheadDefinition) MyDefinitionManager.Static.GetCubeBlockDefinition(id);


                var pos = player.GetPosition() + Vector3.Up * 100;
                MatrixD world = MatrixD.CreateWorld(pos, Vector3.Forward, Vector3.Up);
                Vector3 color = Color.Red.ColorToHSV();
                MyCubeBuilder.SpawnDynamicGrid(definitionBase, player.Character.Entity, world, color, MyStringHash.NullOrEmpty);
            }
        }
    }
    
}