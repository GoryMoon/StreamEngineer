using System.Collections.Generic;
using System.Linq;
using GoryMoon.StreamEngineer.Config;
using GoryMoon.StreamEngineer.Data;
using Newtonsoft.Json.Linq;

namespace GoryMoon.StreamEngineer.Actions
{
    public class DataHandler : BaseDataHandler
    {
        private readonly ActionHandler _actionHandler;

        public DataHandler(string path, IDataPlugin plugin) : base(plugin)
        {
            _actionHandler = new ActionHandler(path, "events.json", Plugin.Logger);
            _actionHandler.AddAction("meteors", typeof(MeteorAction));

            _actionHandler.AddAction("power_on", typeof(EnablePowerAction));
            _actionHandler.AddAction("power_off", typeof(DisablePowerAction));
            _actionHandler.AddAction("power_toggle", typeof(TogglePowerAction));
            
            _actionHandler.AddAction("toggle_power", typeof(TogglePowerAction));
            _actionHandler.AddAction("enable_power", typeof(EnablePowerAction));
            _actionHandler.AddAction("disable_power", typeof(DisablePowerAction));

            _actionHandler.AddAction("refill", typeof(RefillAction));
            _actionHandler.AddAction("power_up", typeof(PowerUpAction));
            _actionHandler.AddAction("power_down", typeof(PowerDownAction));

            _actionHandler.AddAction("toggle_dampeners", typeof(ToggleDampenersAction));
            _actionHandler.AddAction("enable_dampeners", typeof(EnableDampenersAction));
            _actionHandler.AddAction("disable_dampeners", typeof(DisableDampenersAction));

            _actionHandler.AddAction("toggle_thrusters", typeof(ToggleThrustersAction));
            _actionHandler.AddAction("enable_thrusters", typeof(EnableThrustersAction));
            _actionHandler.AddAction("disable_thrusters", typeof(DisableThrustersAction));

            _actionHandler.AddAction("toggle_helmet", typeof(ToggleHelmetAction));
            _actionHandler.AddAction("enable_helmet", typeof(EnableHelmetAction));
            _actionHandler.AddAction("disable_helmet", typeof(DisableHelmetAction));

            _actionHandler.AddAction("fulfill_buildplanner", typeof(FulfillBuildPlannerAction));
            _actionHandler.AddAction("give_item", typeof(GiveItemAction));
            _actionHandler.AddAction("inventory_bomb", typeof(InventoryBombAction));
            
            _actionHandler.AddAction("multi", typeof(MultiActions));
            _actionHandler.AddAction("random", typeof(RandomAction));
            _actionHandler.AddAction("warhead", typeof(WarheadAction));
            _actionHandler.AddAction("spawn_drone", typeof(SpawnDroneAction));
            _actionHandler.AddAction("snap", typeof(SnapAction));
            
            _actionHandler.AddAction("charge_jump_drive", typeof(ChargeJumpDriveAction));
            //_actionHandler.AddAction("start_jump_drive", typeof(StartJumpDriveAction));
            _actionHandler.StartWatching();
        }
        public override void Dispose()
        {
            _actionHandler.Dispose();
        }

        private void SendMessage(string msg, bool alwaysSendMessage, List<BaseAction> actions)
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

        public override void OnDonation(string name, int amount, string formattedAmount)
        {
            var actions = GetAndExecute(new Data.Data {Type = EventType.Donation, Amount = amount});
            var messageEvent = Configuration.Plugin.Get(c => c.Events.Donation);
            SendMessage(string.Format(messageEvent.Message, name, formattedAmount), messageEvent.AlwaysSendMessage,
                actions);
        }

        public override void OnTwitchSubscription(string name, int months, string tier, bool resub)
        {
            var actions = GetAndExecute(new Data.Data {Type = EventType.TwitchSubscription, Amount = months, Tier = tier == "2000" ? 2: tier == "3000" ? 3: 1});
            var messages = Configuration.Plugin.Get(c => c.Events.TwitchSubscription);

            var msg = months == 1 ? messages.NewMessage : messages.ResubMessage;
            string tierMsg;
            switch (tier)
            {
                case "2000":
                    tierMsg = messages.Tier2;
                    break;
                case "3000":
                    tierMsg = messages.Tier3;
                    break;
                default:
                    tierMsg = messages.Tier1;
                    break;
            }

            SendMessage(string.Format(msg, name, tierMsg, months), messages.AlwaysSendMessage, actions);
        }

        public override void OnTwitchBits(string name, int amount)
        {
            var actions = GetAndExecute(new Data.Data {Type = EventType.TwitchBits, Amount = amount});

            var messageEvent = Configuration.Plugin.Get(c => c.Events.TwitchBits);
            SendMessage(string.Format(messageEvent.Message, name, amount), messageEvent.AlwaysSendMessage, actions);
        }

        public override void OnTwitchFollow(string name)
        {
            var actions = GetAndExecute(new Data.Data {Type = EventType.TwitchFollow, Amount = -1});

            var messageEvent = Configuration.Plugin.Get(c => c.Events.TwitchFollowed);
            SendMessage(string.Format(messageEvent.Message, name), messageEvent.AlwaysSendMessage, actions);
        }

        public override void OnTwitchHost(string name, int viewers)
        {
            var actions = GetAndExecute(new Data.Data {Type = EventType.TwitchHost, Amount = viewers});
            var messageEvent = Configuration.Plugin.Get(c => c.Events.TwitchHost);
            SendMessage(string.Format(messageEvent.Message, name, viewers), messageEvent.AlwaysSendMessage, actions);
        }

        public override void OnTwitchRaid(string name, int viewers)
        {
            var actions = GetAndExecute(new Data.Data {Type = EventType.TwitchRaid, Amount = viewers});
            var messageEvent = Configuration.Plugin.Get(c => c.Events.TwitchRaid);
            SendMessage(string.Format(messageEvent.Message, name, viewers), messageEvent.AlwaysSendMessage, actions);
        }

        public override void OnYoutubeSubscription(string name)
        {
            var actions = GetAndExecute(new Data.Data {Type = EventType.YoutubeSubscription, Amount = -1});

            var messageEvent = Configuration.Plugin.Get(c => c.Events.YoutubeSubscription);
            SendMessage(string.Format(messageEvent.Message, name), messageEvent.AlwaysSendMessage, actions);
        }

        public override void OnYoutubeSponsor(string name, int months)
        {
            var actions = GetAndExecute(new Data.Data {Type = EventType.YoutubeSponsor, Amount = months});

            var messages = Configuration.Plugin.Get(c => c.Events.YoutubeSponsor);
            var msg = months == 1 ? messages.NewMessage : messages.ResubMessage;
            SendMessage(string.Format(msg, name, months), messages.AlwaysSendMessage, actions);
        }

        public override void OnYoutubeSuperchat(string name, int amount, string formatted)
        {
            var actions = GetAndExecute(new Data.Data {Type = EventType.YoutubeSuperchat, Amount = amount});
            var messageEvent = Configuration.Plugin.Get(c => c.Events.YoutubeSuperchat);
            SendMessage(string.Format(messageEvent.Message, name, formatted), messageEvent.AlwaysSendMessage, actions);
        }

        public override void OnMixerSubscription(string name, int months)
        {
            var actions = GetAndExecute(new Data.Data {Type = EventType.MixerSubscription, Amount = months});

            var messages = Configuration.Plugin.Get(c => c.Events.MixerSubscription);
            var msg = months == 1 ? messages.NewMessage : messages.ResubMessage;
            SendMessage(string.Format(msg, name, months), messages.AlwaysSendMessage, actions);
        }

        public override void OnMixerFollow(string name)
        {
            var actions = GetAndExecute(new Data.Data {Type = EventType.MixerFollow, Amount = -1});

            var messageEvent = Configuration.Plugin.Get(c => c.Events.MixerFollowMessage);
            SendMessage(string.Format(messageEvent.Message, name), messageEvent.AlwaysSendMessage, actions);
        }

        public override void OnMixerHost(string name, int viewers)
        {
            var actions = GetAndExecute(new Data.Data {Type = EventType.MixerHost, Amount = viewers});
            var messageEvent = Configuration.Plugin.Get(c => c.Events.MixerHostMessage);
            SendMessage(string.Format(messageEvent.Message, name, viewers), messageEvent.AlwaysSendMessage, actions);
        }
        
        public override void OnTwitchExtension(string name, int amount, string action, JToken settings)
        {
            if (_actionHandler.GetAction(action, settings, out var baseAction))
            {
                var data = new Data.Data {Type = EventType.TwitchExtension, Amount = amount};
                baseAction.Execute(data, GetParams(data));
                if (baseAction.Message != null && baseAction.Message.Trim().Length > 0)
                {
                    var messageEvent = Configuration.Plugin.Get(c => c.Events.TwitchExtension);
                    SendMessage(string.Format(messageEvent.WithMessage, name, amount), true, new List<BaseAction> {baseAction});
                }
                else
                {
                    var messageEvent = Configuration.Plugin.Get(c => c.Events.TwitchExtension);
                    SendMessage(string.Format(messageEvent.WithoutMessage, name, amount), true, new List<BaseAction> {baseAction});
                }
            }
        }
        
        public override void OnTwitchChannelPoints(string name, string id)
        {
            var actions = GetAndExecute(new Data.Data {Type = EventType.TwitchChannelPoints, Id = id});

            var messageEvent = Configuration.Plugin.Get(c => c.Events.TwitchChannelPoints);
            SendMessage(string.Format(messageEvent.Message, name), messageEvent.AlwaysSendMessage, actions);
        }
    }
}