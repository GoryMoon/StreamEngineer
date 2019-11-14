using System;
using System.Linq;
using GoryMoon.StreamEngineer.Data;
using Sandbox.Game.Entities;
using VRage.Game.ModAPI;
using VRageMath;

namespace GoryMoon.StreamEngineer.Actions
{
    public class SnapAction: BaseAction
    {
        public bool Vehicle { get; set; } = true;
        
        private readonly Random _random = new Random();
        
        public override void Execute(Data.Data data)
        {
            SessionHandler.EnqueueAction(() =>
            {
                var player = Utils.GetPlayer();
                if (player != null)
                {
                    if (Vehicle)
                    {
                        var controlledEntity = player.Controller.ControlledEntity;
                        if (controlledEntity is MyShipController controller)
                        {
                            var grid = controller.CubeGrid;
                            grid.CubeBlocks.Where(block => _random.Next(2) == 0).ToList()
                                .ForEach(block => grid.RemoveBlock(block));
                        }
                    }

                    if (_random.Next(2) == 0)
                    {
                        player.Character.Kill(true, new MyDamageInformation());
                    }
                    ActionNotification.SendActionMessage("SNAP!", Color.Red, "ArcHudGPSNotification1");
                }
            });
        }
    }
}