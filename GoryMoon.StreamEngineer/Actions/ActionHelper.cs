using System;
using Sandbox.Engine.Multiplayer;
using Sandbox.ModAPI;
using VRage.Network;

namespace GoryMoon.StreamEngineer.Actions
{
    [StaticEventOwner]
    public class ActionHelper
    {
        [Serializable]
        public enum ActionEnum
        {
            Toggle,
            Enable,
            Disable
        }

        private static void Call(Action<ActionEnum> action, ActionEnum actionEnum, ulong id)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                action.Invoke(actionEnum);
            }
            else
            {
                MyMultiplayer.RaiseStaticEvent(owner => action, actionEnum, new EndpointId(id));
            }
        }

        public static readonly Action<ActionEnum, ulong> SetPower = (action, id) => Call(SetPowerInternal, action, id);
        public static readonly Action<ActionEnum, ulong> SetDampener = (action, id) => Call(SetDampenerInternal, action, id);
        public static readonly Action<ActionEnum, ulong> SetThrusters = (action, id) => Call(SetThrustersInternal, action, id);
        public static readonly Action<ActionEnum, ulong> SetHelmet = (action, id) => Call(SetHelmetInternal, action, id);
                
        [Event(null, 481)]
        [Reliable]
        [Client]
        private static void SetPowerInternal(ActionEnum action)
        {
            var entity = MyAPIGateway.Session.Player.Controller.ControlledEntity;
            switch (action)
            {
                case ActionEnum.Disable when !entity.EnabledReactors:
                case ActionEnum.Enable when entity.EnabledReactors:
                    return;
                default:
                    entity.SwitchReactors();
                    break;
            }
        }

        [Event(null, 481)]
        [Reliable]
        [Client]
        private static void SetDampenerInternal(ActionEnum action)
        {
            var entity = MyAPIGateway.Session.Player.Controller.ControlledEntity;
            switch (action)
            {
                case ActionEnum.Disable when !entity.EnabledDamping:
                case ActionEnum.Enable when entity.EnabledDamping:
                    return;
                default:
                    entity.SwitchDamping();
                    break;
            }
        }
        
        [Event(null, 481)]
        [Reliable]
        [Client]
        private static void SetThrustersInternal(ActionEnum action)
        {
            var entity = MyAPIGateway.Session.Player.Controller.ControlledEntity;
            switch (action)
            {
                case ActionEnum.Disable when !entity.EnabledThrusts:
                case ActionEnum.Enable when entity.EnabledThrusts:
                    return;
                default:
                    entity.SwitchThrusts();
                    break;
            }
        }
        
        [Event(null, 481)]
        [Reliable]
        [Client]
        private static void SetHelmetInternal(ActionEnum action)
        {
            var entity = MyAPIGateway.Session.Player.Controller.ControlledEntity;
            switch (action)
            {
                case ActionEnum.Disable when !entity.EnabledHelmet:
                case ActionEnum.Enable when entity.EnabledHelmet:
                    return;
                default:
                    entity.SwitchHelmet();
                    break;
            }
        }
    }
}