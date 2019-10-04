using System;
using Sandbox;
using Sandbox.Engine.Multiplayer;
using VRage.Collections;
using VRage.Game.Components;

namespace GoryMoon.StreamEngineer
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class SessionHandler : MySessionComponentBase
    {
        private static readonly MyConcurrentQueue<Action> ActionQueue = new MyConcurrentQueue<Action>(32);
        private int _mUpdateCounter;

        public override bool IsRequiredByGame => true;

        protected override void UnloadData()
        {
            Plugin.StopService();
            ActionQueue.Clear();
        }

        public override void BeforeStart()
        {
            MyMultiplayer.ReplicationLayer.RegisterFromAssembly(typeof(Plugin).Assembly);
            Plugin.StartService();
        }

        public override void UpdateBeforeSimulation()
        {
            ++_mUpdateCounter;
            if (ActionQueue.Count > 0 && _mUpdateCounter % 30 == 0)
            {
                _mUpdateCounter = 0;
                if (ActionQueue.TryDequeue(out var action)) action.Invoke();
            }
        }

        public static void EnqueueAction(Action action)
        {
            RunOnMainThread(() => ActionQueue.Enqueue(action));
        }

        public static void RunOnMainThread(Action action)
        {
            MySandboxGame.Static.Invoke(action, "SessionHandler.RunOnMainThread");
        }

        public static void EnqueueMeteors()
        {
            EnqueueAction(() => MeteorShower.MeteorWave());
        }
    }
}