using System.Collections.Generic;
using System.Linq;
using GoryMoon.StreamEngineer.Config;
using GoryMoon.StreamEngineer.Data;
using Newtonsoft.Json.Linq;

namespace GoryMoon.StreamEngineer.Actions
{
    public partial class DataHandler : BaseDataHandler
    {
        private readonly ActionHandler _actionHandler;

        public DataHandler(string path, IDataPlugin plugin) : base(plugin)
        {
            _actionHandler = new ActionHandler(path, "events.json", Plugin.Logger);
            _actionHandler.AddAction(typeof(MeteorAction));

            _actionHandler.AddAction(typeof(EnablePowerAction), "power_on");
            _actionHandler.AddAction(typeof(DisablePowerAction), "power_off");
            _actionHandler.AddAction(typeof(TogglePowerAction), "power_toggle");
            
            _actionHandler.AddAction(typeof(TogglePowerAction));
            _actionHandler.AddAction(typeof(EnablePowerAction));
            _actionHandler.AddAction(typeof(DisablePowerAction));

            _actionHandler.AddAction(typeof(RefillAction));
            _actionHandler.AddAction(typeof(PowerUpAction));
            _actionHandler.AddAction(typeof(PowerDownAction));

            _actionHandler.AddAction(typeof(ToggleDampenersAction));
            _actionHandler.AddAction(typeof(EnableDampenersAction));
            _actionHandler.AddAction(typeof(DisableDampenersAction));

            _actionHandler.AddAction(typeof(ToggleThrustersAction));
            _actionHandler.AddAction(typeof(EnableThrustersAction));
            _actionHandler.AddAction(typeof(DisableThrustersAction));

            _actionHandler.AddAction(typeof(ToggleHelmetAction));
            _actionHandler.AddAction(typeof(EnableHelmetAction));
            _actionHandler.AddAction(typeof(DisableHelmetAction));

            _actionHandler.AddAction(typeof(FulfillBuildPlannerAction));
            _actionHandler.AddAction(typeof(GiveItemAction));
            _actionHandler.AddAction(typeof(InventoryBombAction));
            
            _actionHandler.AddAction(typeof(MultiActions));
            _actionHandler.AddAction(typeof(RandomAction));
            _actionHandler.AddAction(typeof(WarheadAction));
            _actionHandler.AddAction(typeof(SpawnDroneAction));
            _actionHandler.AddAction(typeof(SnapAction));
            
            _actionHandler.AddAction(typeof(ChargeJumpDriveAction));
            //_actionHandler.AddAction(typeof(StartJumpDriveAction));
            
            _actionHandler.AddAction(typeof(ChangeWeatherAction));
            _actionHandler.AddAction(typeof(SmiteAction));
            _actionHandler.AddAction(typeof(CommandAction));

            _actionHandler.PrintActionTypes();
            _actionHandler.StartWatching();
        }
        public override void Dispose()
        {
            _actionHandler.Dispose();
        }

        private void SendMessage(string msg, bool alwaysSendMessage, IReadOnlyCollection<BaseAction> actions)
        {
            SessionHandler.RunOnMainThread(() =>
            {
                var actionMessage = GetMessage(actions);
                msg += actionMessage;
                Plugin.Logger.WriteLine(msg);
                if (actions.Count <= 0 && !alwaysSendMessage) return;
                ActionNotification.SendActionMessage(msg, null);
                Utils.SendChat(msg);
            });
        }

        private string GetMessage(IReadOnlyCollection<BaseAction> actions)
        {
            return actions.Count > 0 ? " " + string.Join(" ", actions.Select(action => action.Message ?? "")) : "";
        }

        public List<BaseAction> GetAndExecute(Data.Data data)
        {
            var actions = _actionHandler.GetActions(data);
            var param = GetParams(data);
            if (EventType.TwitchSubscription.Equals(data.Type))
            {
                param.Add("tier", data.Tier);
            }
            actions.ForEach(action => action.Execute(data, param));
            return actions;
        }
        
        public void Execute(string type, JToken token, Data.Data data)
        {
            _actionHandler.GetAction(type, token, out var action);
            action.Execute(data, GetParams(data));
        }

        public List<string> GetActionNames() => _actionHandler.GetActionNames();

        public override void OnDonation(string name, int amount, string formattedAmount)
        {
            var actions = GetAndExecute(new Data.Data {Type = EventType.Donation, Amount = amount});
            var messageEvent = Configuration.Plugin.Get(c => c.Events.Donation);
            SendMessage(string.Format(messageEvent.Message, name, formattedAmount), messageEvent.AlwaysSendMessage,
                actions);
        }
    }
}