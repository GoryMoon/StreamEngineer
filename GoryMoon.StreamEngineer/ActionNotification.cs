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
using VRage.Serialization;
using VRage.Utils;
using VRageMath;
using Game = Sandbox.Engine.Platform.Game;

namespace GoryMoon.StreamEngineer
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    [StaticEventOwner]
    public class ActionNotification : MySessionComponentBase
    {
        private static readonly MyConcurrentQueue<Message> MessageQueue = new MyConcurrentQueue<Message>();
        private static Message? _currentMessage;
        
        private int _currentDisplayTime;
        private Vector2 _position;

        public override void LoadData()
        {
            _position = MyNotificationConstants.DEFAULT_NOTIFICATION_MESSAGE_NORMALIZED_POSITION;
            _position.Y -= 0.25F;
        }

        public override bool IsRequiredByGame => true;

        public static void SendActionMessage(string message, Color? color, string sound = "ArcNewItemImpact")
        {
            if (Sync.IsServer)
            {
                if (!Game.IsDedicated)
                {
                    AddActionMessage(message, color, sound);
                }
                MyMultiplayer.RaiseStaticEvent(x => AddActionMessage, message, color, sound, new EndpointId(), new Vector3D?());
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
                _currentMessage = MessageQueue.Dequeue();
                MyAudio.Static.PlaySound(MySoundPair.GetCueId(_currentMessage.Value.Sound));
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
            if (_currentMessage.HasValue)
            {
                MyGuiManager.DrawString("Debug", _currentMessage.Value.Text, _position,
                    MyGuiSandbox.GetDefaultTextScaleWithLanguage() * 1.2f, _currentMessage.Value.Color,
                    MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyVideoSettingsManager.IsTripleHead());
            }
        }

        [Event(null, 53)]
        [Reliable]
        [Broadcast]
        private static void AddActionMessage(string message, [Nullable] Color? color, string sound)
        {
            MessageQueue.Enqueue(new Message()
                {Text = new StringBuilder(message), Color = color ?? Color.PaleGoldenrod, Sound = sound});
        }
        
        private struct Message
        {
            public StringBuilder Text;
            public Color Color;
            public string Sound;
        }
    }
}