using System.Text;
using Sandbox;
using Sandbox.Engine.Multiplayer;
using Sandbox.Engine.Platform.VideoMode;
using Sandbox.Game.Entities;
using Sandbox.Game.Multiplayer;
using Sandbox.Graphics;
using Sandbox.Graphics.GUI;
using VRage.Audio;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Network;
using VRage.Utils;
using VRageMath;
using Game = Sandbox.Engine.Platform.Game;

namespace GoryMoon.StreamEngineer
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    [StaticEventOwner]
    public class ActionNotification : MySessionComponentBase
    {
        private static readonly MyConcurrentQueue<string> MessageQueue = new MyConcurrentQueue<string>();
        private static StringBuilder _currentMessage;
        
        private int _currentDisplayTime;
        private Vector2 _position;

        public override void LoadData()
        {
            _position = MyNotificationConstants.DEFAULT_NOTIFICATION_MESSAGE_NORMALIZED_POSITION;
            _position.Y -= 0.25F;
        }

        public override bool IsRequiredByGame => true;

        public static void SendActionMessage(string message)
        {
            if (Sync.IsServer)
            {
                if (!Game.IsDedicated)
                {
                    AddActionMessage(message);
                }
                MyMultiplayer.RaiseStaticEvent(x => AddActionMessage, message, new EndpointId(), new Vector3D?());
            }
        }

        public override void UpdateAfterSimulation()
        {
            if (!MySandboxGame.IsGameReady)
                return;
            if (Game.IsDedicated)
                return;

            if (_currentDisplayTime <= 0 && MessageQueue.Count > 0)
            {
                _currentDisplayTime = 200;
                _currentMessage = new StringBuilder(MessageQueue.Dequeue());
                MyAudio.Static.PlaySound(MySoundPair.GetCueId("ArcNewItemImpact"));
            }
            else if (_currentDisplayTime > 0)
            {
                _currentDisplayTime--;
                if (_currentDisplayTime <= 0)
                {
                    _currentMessage = null;
                }
            }
        }

        public override void Draw()
        {
            if (_currentMessage != null)
            {
                MyGuiManager.DrawString("Debug", _currentMessage, _position,
                    MyGuiSandbox.GetDefaultTextScaleWithLanguage() * 1.2f, Color.PaleGoldenrod,
                    MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyVideoSettingsManager.IsTripleHead());
            }
        }

        [Event(null, 53)]
        [Reliable]
        [Broadcast]
        private static void AddActionMessage(string message)
        {
            MessageQueue.Enqueue(message);
        }
    }
}