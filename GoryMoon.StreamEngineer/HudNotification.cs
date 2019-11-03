using System.Collections.Generic;
using Sandbox;
using Sandbox.Engine.Platform;
using Sandbox.Game.Gui;
using Sandbox.Game.GUI;
using Sandbox.Game.Localization;
using Sandbox.Game.World;
using VRage.Audio;
using VRage.Game.Components;
using VRage.Utils;
using VRageMath;

namespace GoryMoon.StreamEngineer
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class HudNotification : MySessionComponentBase
    {
        private static readonly int FRAMES_BETWEEN_UPDATE = 30;
        private static readonly List<MyGuiSounds> M_SOUND_QUEUE = new List<MyGuiSounds>();
        private static int _mLastSoundPlayed;
        private static IMySourceVoice _mSound;

        private int _mMsSinceLastCuePlayed;
        private int _mUpdateCounter;

        private WarningState _mWarningState;
        private MyHudNotification _notification;

        public override bool IsRequiredByGame => true;

        public HudNotification()
        {
            Static = this;
        }

        public static HudNotification Static { get; set; }

        public override void LoadData()
        {
            _notification = new MyHudNotification(MySpaceTexts.NotificationMeteorInbound, 5000, "Red",
                MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Important);
        }

        public static void EnqueueSound(MyGuiSounds sound)
        {
            if (!MyGuiAudio.HudWarnings)
                return;
            if ((_mSound == null || !_mSound.IsPlaying) &&
                MySandboxGame.TotalGamePlayTimeInMilliseconds - _mLastSoundPlayed > 5000)
            {
                _mSound = MyGuiAudio.PlaySound(sound);
                _mLastSoundPlayed = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            }
            else
            {
                M_SOUND_QUEUE.Add(sound);
            }
        }

        public override void UpdateAfterSimulation()
        {
            if (!MySandboxGame.IsGameReady)
                return;
            if (Game.IsDedicated)
                return;
            ++_mUpdateCounter;
            if (_mUpdateCounter % FRAMES_BETWEEN_UPDATE != 0) return;

            if (!MeteorShower.CurrentTarget.HasValue || MySession.Static.ControlledEntity == null)
            {
                M_SOUND_QUEUE.Clear();
                MyHud.Notifications.Remove(_notification);
                _mWarningState = WarningState.NotStarted;
                return;
            }

            var cue = MyGuiSounds.HudVocMeteorInbound;
            var currentTarget = MeteorShower.CurrentTarget;
            var num1 = (double) Vector3.Distance(currentTarget.Value.Center,
                MySession.Static.ControlledEntity.Entity.PositionComp.GetPosition());
            currentTarget = MeteorShower.CurrentTarget;
            var num2 = 2.0 * currentTarget.Value.Radius + 500.0;
            if (num1 < num2)
            {
                _mMsSinceLastCuePlayed += 16 * FRAMES_BETWEEN_UPDATE;
                switch (_mWarningState)
                {
                    case WarningState.NotStarted:
                        MyHud.Notifications.Add(_notification);
                        _mWarningState = WarningState.Started;
                        break;
                    case WarningState.Started:
                        EnqueueSound(cue);
                        _mWarningState = WarningState.PLAYED;
                        _mMsSinceLastCuePlayed = 0;
                        break;
                    case WarningState.PLAYED:
                        break;
                }
            }
            else
            {
                M_SOUND_QUEUE.Clear();
                MyHud.Notifications.Remove(_notification);
                _mWarningState = WarningState.NotStarted;
            }


            if (M_SOUND_QUEUE.Count <= 0 || MySandboxGame.TotalGamePlayTimeInMilliseconds - _mLastSoundPlayed <= 5000)
                return;
            _mLastSoundPlayed = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            _mSound = MyGuiAudio.PlaySound(M_SOUND_QUEUE[0]);
            M_SOUND_QUEUE.RemoveAt(0);
        }

        private enum WarningState
        {
            NotStarted,
            Started,
            PLAYED
        }
    }
}