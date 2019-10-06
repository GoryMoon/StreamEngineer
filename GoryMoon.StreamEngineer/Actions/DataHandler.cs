using System;
using System.Collections.Generic;
using System.Linq;
using GoryMoon.StreamEngineer.Config;
using GoryMoon.StreamEngineer.Data;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Gui;

namespace GoryMoon.StreamEngineer.Actions
{
    public class DataHandler: BaseDataHandler
    {
        private ActionHandler _actionHandler;
        
        public DataHandler(string path) : base(Plugin.Static.Logger)
        {
            _actionHandler = new ActionHandler(path, "events.json", Logger);
            _actionHandler.AddAction("meteors", typeof(MeteorAction));
            _actionHandler.AddAction("power_off", typeof(PowerOffAction));
            _actionHandler.AddAction("refill", typeof(RefillAction));
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
                msg += GetMessage(actions);
                Logger.WriteLine(msg);
                if (actions.Count <= 0 && !alwaysSendMessage) return;

                if (MyMultiplayer.Static != null)
                    MyMultiplayer.Static.SendChatMessageScripted(msg, ChatChannel.GlobalScripted, 0, "[StreamEngineer]");
                else
                    MyHud.Chat.ShowMessageScripted("[StreamEngineer]", msg);
            });
        }

        private string GetMessage(IReadOnlyCollection<BaseAction> actions)
        {
            return actions.Count > 0 ? " " + string.Join(" ", actions.Select(action => action.Message)) : "";
        }

        private List<BaseAction> GetAndExecute(Data.Data data)
        {
            var actions = _actionHandler.GetActions(data);
            actions.ForEach(action => action.Execute(data));
            return actions;
        }

        public override void OnDonation(string name, int amount, string formattedAmount)
        {
            var actions = GetAndExecute(new Data.Data {Type = EventType.Donation, Amount = amount});
            var messageEvent = Configuration.Plugin.Get(c => c.Events.Donation);
            SendMessage(string.Format(messageEvent.Message, name, formattedAmount), messageEvent.AlwaysSendMessage, actions);
        }

        public override void OnTwitchSubscription(string name, int months, string tier, bool resub)
        {
            var actions = GetAndExecute(new Data.Data {Type = EventType.TwitchSubscription, Amount = months});
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
    }
}