using System;
using System.Collections.Generic;
using System.Linq;
using GoryMoon.StreamEngineer.Config;
using Sandbox;
using Sandbox.Engine.Multiplayer;
using Sandbox.Engine.Physics;
using Sandbox.Engine.Utils;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.GameSystems;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.SessionComponents;
using Sandbox.Game.World;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Network;
using VRage.Serialization;
using VRage.Utils;
using VRageMath;
using VRageRender;

namespace GoryMoon.StreamEngineer
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    [StaticEventOwner]
    public class MeteorShower : MySessionComponentBase
    {
        private static readonly int WavesInShower = 1;
        private static readonly double HorizonAngleFromZenithRatio = Math.Sin(0.35);
        private static readonly double MeteorBlurKoef = 2.5;
        private static readonly List<MyEntity> MeteorList = new List<MyEntity>();
        private static List<MyEntity> _mTmpEntityList = new List<MyEntity>();
        private static readonly List<BoundingSphereD> TargetList = new List<BoundingSphereD>();
        private static Vector3 _mRightVector = Vector3.Zero;
        private static readonly List<MyCubeGrid> TmpHitGroup = new List<MyCubeGrid>();

        private static Vector3D _mTgtPos;
        private static Vector3D _mNormalSun;
        private static Vector3D _mPltTgtDir;
        private static Vector3D _mMirrorDir;
        private static int _mWaveCounter;
        private static BoundingSphereD? _mCurrentTarget;
        private static Vector3 _mDownVector;
        private static int _meteoroidCount;
        private static Vector3D _mMeteorHitPos;


        public static BoundingSphereD? CurrentTarget
        {
            get => _mCurrentTarget;
            set => _mCurrentTarget = value;
        }

        public override bool IsRequiredByGame => MyPerGameSettings.Game == GameEnum.SE_GAME;

        public override void LoadData()
        {
            _mWaveCounter = -1;
        }

        protected override void UnloadData()
        {
            foreach (var meteor in MeteorList)
                if (!meteor.MarkedForClose)
                    meteor.Close();
            MeteorList.Clear();
            _mCurrentTarget = new BoundingSphereD?();
            TargetList.Clear();
        }

        private static void MeteorWaveInternal()
        {
            if (MySession.Static.EnvironmentHostility != MyEnvironmentHostilityEnum.SAFE)
            {
                if (!Sync.IsServer)
                    return;
                ++_mWaveCounter;
                if (_mWaveCounter == 0)
                {
                    ClearMeteorList();
                    if (TargetList.Count == 0)
                    {
                        GetTargets();
                        if (TargetList.Count == 0)
                        {
                            _mWaveCounter = WavesInShower + 1;
                            RescheduleEvent();
                            return;
                        }
                    }

                    _mCurrentTarget = TargetList.ElementAt(MyUtils.GetRandomInt(TargetList.Count - 1));
                    MyMultiplayer.RaiseStaticEvent(
                        x => UpdateShowerTarget, _mCurrentTarget, new EndpointId(),
                        new Vector3D?());
                    TargetList.Remove(_mCurrentTarget.Value);
                    _meteoroidCount = (int) (Math.Pow(_mCurrentTarget.Value.Radius, 2.0) * Math.PI / 6000.0);
                    _meteoroidCount /= MySession.Static.EnvironmentHostility == MyEnvironmentHostilityEnum.CATACLYSM ||
                                       MySession.Static.EnvironmentHostility ==
                                       MyEnvironmentHostilityEnum.CATACLYSM_UNREAL
                        ? 1
                        : 8;
                    _meteoroidCount = MathHelper.Clamp(_meteoroidCount, 1, 30);
                }

                RescheduleEvent();
                CheckTargetValid();
                if (_mWaveCounter < 0)
                    return;
                StartWave();
            }
        }

        private static void StartWave()
        {
            if (!_mCurrentTarget.HasValue)
                return;
            var correctedDirection = GetCorrectedDirection(MySector.DirectionToSunNormalized);
            SetupDirVectors(correctedDirection);
            var randomFloat1 =
                MyUtils.GetRandomFloat(Math.Min(2, _meteoroidCount - 3), _meteoroidCount + 3);
            var circleNormalized1 = MyUtils.GetRandomVector3CircleNormalized();
            var randomFloat2 = MyUtils.GetRandomFloat(0.0f, 1f);
            var vector3D1 = (Vector3D) (circleNormalized1.X * _mRightVector + circleNormalized1.Z * _mDownVector);
            var vector3D2 = _mCurrentTarget.Value.Center + Math.Pow(randomFloat2, 0.699999988079071) *
                            _mCurrentTarget.Value.Radius * vector3D1 * MeteorBlurKoef;
            var vector3D3 =
                -Vector3D.Normalize(MyGravityProviderSystem.CalculateNaturalGravityInPoint(vector3D2));
            if (vector3D3 != Vector3D.Zero)
            {
                var nullable = MyPhysics.CastRay(vector3D2 + vector3D3 * 3000.0, vector3D2, 15);
                if (nullable.HasValue)
                    vector3D2 = nullable.Value.Position;
            }

            _mMeteorHitPos = vector3D2;
            for (var index = 0; (double) index < (double) randomFloat1; ++index)
            {
                var circleNormalized2 = MyUtils.GetRandomVector3CircleNormalized();
                var randomFloat3 = MyUtils.GetRandomFloat(0.0f, 1f);
                var vector3D4 = (Vector3D) (circleNormalized2.X * _mRightVector + circleNormalized2.Z * _mDownVector);
                vector3D2 += Math.Pow(randomFloat3, 0.699999988079071) * _mCurrentTarget.Value.Radius *
                             vector3D4;
                var vector3 = correctedDirection * (2000 + 100 * index);
                var circleNormalized3 = MyUtils.GetRandomVector3CircleNormalized();
                var vector3D5 = (Vector3D) (circleNormalized3.X * _mRightVector + circleNormalized3.Z * _mDownVector);
                var position = vector3D2 + vector3 +
                               Math.Tan(MyUtils.GetRandomFloat(0.0f, 0.1745329f)) * vector3D5;
                var entity = MyMeteor.SpawnRandom(position, Vector3.Normalize(vector3D2 - position));
                MeteorList.Add(entity);
            }
            
            _mRightVector = Vector3.Zero;
        }

        private static Vector3 GetCorrectedDirection(Vector3 direction)
        {
            var v = direction;
            if (!_mCurrentTarget.HasValue)
                return v;
            var center = _mCurrentTarget.Value.Center;
            _mTgtPos = center;
            if (!MyGravityProviderSystem.IsPositionInNaturalGravity(center))
                return v;
            var vector3D1 =
                -Vector3D.Normalize(MyGravityProviderSystem.CalculateNaturalGravityInPoint(center));
            var vector3D2 = Vector3D.Normalize(Vector3D.Cross(vector3D1, v));
            var vector3D3 = Vector3D.Normalize(Vector3D.Cross(vector3D2, vector3D1));
            _mMirrorDir = vector3D3;
            _mPltTgtDir = vector3D1;
            _mNormalSun = vector3D2;
            var num = vector3D1.Dot(v);
            if (num < -HorizonAngleFromZenithRatio)
                return Vector3D.Reflect(-v, vector3D3);
            if (num >= HorizonAngleFromZenithRatio)
                return v;
            var fromAxisAngle = MatrixD.CreateFromAxisAngle(vector3D2, -Math.Asin(HorizonAngleFromZenithRatio));
            return Vector3D.Transform(vector3D3, fromAxisAngle);
        }

        public override void Draw()
        {
            if (!MyDebugDrawSettings.ENABLE_DEBUG_DRAW)
                return;
            var correctedDirection = (Vector3D) GetCorrectedDirection(MySector.DirectionToSunNormalized);
            MyRenderProxy.DebugDrawPoint(_mMeteorHitPos, Color.White, false);
            MyRenderProxy.DebugDrawText3D(_mMeteorHitPos, "Hit position", Color.White, 0.5f, false);
            MyRenderProxy.DebugDrawLine3D(_mTgtPos, _mTgtPos + 10f * MySector.DirectionToSunNormalized, Color.Yellow,
                Color.Yellow, false);
            MyRenderProxy.DebugDrawText3D(_mTgtPos + 10f * MySector.DirectionToSunNormalized, "Sun direction (sd)",
                Color.Yellow, 0.5f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM);
            MyRenderProxy.DebugDrawLine3D(_mTgtPos, _mTgtPos + 10.0 * correctedDirection, Color.Red, Color.Red, false);
            MyRenderProxy.DebugDrawText3D(_mTgtPos + 10.0 * correctedDirection, "Current meteorits direction (cd)",
                Color.Red, 0.5f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP);
            if (!MyGravityProviderSystem.IsPositionInNaturalGravity(_mTgtPos))
                return;
            MyRenderProxy.DebugDrawLine3D(_mTgtPos, _mTgtPos + 10.0 * _mNormalSun, Color.Blue, Color.Blue, false);
            MyRenderProxy.DebugDrawText3D(_mTgtPos + 10.0 * _mNormalSun, "Perpendicular to sd and n0 ", Color.Blue,
                0.5f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            MyRenderProxy.DebugDrawLine3D(_mTgtPos, _mTgtPos + 10.0 * _mPltTgtDir, Color.Green, Color.Green, false);
            MyRenderProxy.DebugDrawText3D(_mTgtPos + 10.0 * _mPltTgtDir, "Dir from center of planet to target (n0)",
                Color.Green, 0.5f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            MyRenderProxy.DebugDrawLine3D(_mTgtPos, _mTgtPos + 10.0 * _mMirrorDir, Color.Purple, Color.Purple, false);
            MyRenderProxy.DebugDrawText3D(_mTgtPos + 10.0 * _mMirrorDir, "Horizon in plane n0 and sd (ho)",
                Color.Purple, 0.5f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
        }

        private static void CheckTargetValid()
        {
            if (!_mCurrentTarget.HasValue)
                return;
            _mTmpEntityList.Clear();
            var boundingSphere = _mCurrentTarget.Value;
            _mTmpEntityList = MyEntities.GetEntitiesInSphere(ref boundingSphere);
            if (_mTmpEntityList.OfType<MyCubeGrid>().ToList().Count == 0)
                _mWaveCounter = -1;
            if (_mWaveCounter >= 0) // && MyMusicController.Static != null)
                foreach (var tmpEntity in _mTmpEntityList)
                    if (tmpEntity is MyCharacter && MySession.Static != null &&
                        tmpEntity as MyCharacter == MySession.Static.LocalCharacter)
                        //MyMusicController.Static.MeteorShowerIncoming();
                        break;
            _mTmpEntityList.Clear();
        }

        private static void RescheduleEvent()
        {
            if (_mWaveCounter > WavesInShower)
            {
                _mWaveCounter = -1;
                _mCurrentTarget = new BoundingSphereD?();
                MyMultiplayer.RaiseStaticEvent(x => UpdateShowerTarget, _mCurrentTarget, new EndpointId(),
                    new Vector3D?());
                HudNotification.Static.UpdateAfterSimulation();
                MySandboxGame.Log.WriteLine("Cleared current target");
            }
            else
            {
                Plugin.Static.QueueMeteors();
            }
        }

        private static void GetTargets()
        {
            var player = MySession.Static.Players.GetPlayerByName(Configuration.Config.Get(c => c.SteamName));
            var list = MyEntities.GetEntities().OfType<MyCubeGrid>().ToList();
            for (var index = 0; index < list.Count; ++index)
                if ((list[index].Max - list[index].Min + Vector3I.One).Size < 16 ||
                    !MySessionComponentTriggerSystem.Static.IsAnyTriggerActive(list[index]))
                {
                    list.RemoveAt(index);
                    --index;
                }

            while (list.Count > 0)
            {
                var myCubeGrid1 = list[MyUtils.GetRandomInt(list.Count - 1)];
                TmpHitGroup.Add(myCubeGrid1);
                list.Remove(myCubeGrid1);
                var worldVolume = myCubeGrid1.PositionComp.WorldVolume;
                var flag = true;
                while (flag)
                {
                    flag = false;
                    foreach (var myCubeGrid2 in TmpHitGroup)
                        worldVolume.Include(myCubeGrid2.PositionComp.WorldVolume);
                    TmpHitGroup.Clear();
                    worldVolume.Radius += 10.0;
                    for (var index = 0; index < list.Count; ++index)
                        if (list[index].PositionComp.WorldVolume.Intersects(worldVolume))
                        {
                            flag = true;
                            TmpHitGroup.Add(list[index]);
                            list.RemoveAt(index);
                            --index;
                        }
                }

                worldVolume.Radius += 150.0;
                var containmentType =
                    player == null ? ContainmentType.Contains : worldVolume.Contains(player.GetPosition());
                if (containmentType == ContainmentType.Intersects || containmentType == ContainmentType.Contains)
                    TargetList.Add(worldVolume);
            }
        }

        private static void ClearMeteorList()
        {
            MeteorList.Clear();
        }

        private static void SetupDirVectors(Vector3 direction)
        {
            if (!(_mRightVector == Vector3.Zero))
                return;
            direction.CalculatePerpendicularVector(out _mRightVector);
            _mDownVector = MyUtils.Normalize(Vector3.Cross(direction, _mRightVector));
        }

        public static void MeteorWave()
        {
            MeteorWaveInternal();
        }

        [Event(null, 567)]
        [Reliable]
        [Broadcast]
        private static void UpdateShowerTarget([Serialize(MyObjectFlags.DefaultZero)] BoundingSphereD? target)
        {
            if (target.HasValue)
                CurrentTarget = new BoundingSphereD(target.Value.Center, target.Value.Radius);
            else
                CurrentTarget = new BoundingSphereD?();
        }
    }
}