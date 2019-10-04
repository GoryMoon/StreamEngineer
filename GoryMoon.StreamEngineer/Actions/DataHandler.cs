using System;
using System.Collections.Generic;
using System.Linq;
using GoryMoon.StreamEngineer.Config;
using GoryMoon.StreamEngineer.Data;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Gui;

namespace GoryMoon.StreamEngineer.Actions
{
    public class DataHandler: IDataHandler
    {
        //Normal Actions
        private readonly Dictionary<string, IAction> _actions = new Dictionary<string, IAction>();
        private readonly Dictionary<int, IAction> _bitsActions = new Dictionary<int, IAction>();
        private readonly Dictionary<int, IAction> _twitchHostActions = new Dictionary<int, IAction>();
        private readonly Dictionary<int, IAction> _twitchRaidActions = new Dictionary<int, IAction>();
        private readonly Dictionary<int, IAction> _mixerHostActions = new Dictionary<int, IAction>();
        
        //Fuzzy actions
        private readonly Dictionary<int, IAction> _donationActions = new Dictionary<int, IAction>();
        private readonly Dictionary<int, IAction> _mixSubActions = new Dictionary<int, IAction>();
        private readonly Dictionary<int, IAction> _twitchSubActions = new Dictionary<int, IAction>();
        private readonly Dictionary<int, IAction> _youtubeSubActions = new Dictionary<int, IAction>();
        private bool _fuzzyBits;
        private bool _fuzzyDonations;
        private bool _fuzzySubs;

        private int _lastActionSettingsHash;

        //Actions
        private readonly IAction _meteorAction = new MeteorAction();
        private readonly IAction _warheadAction = new WarheadAction();

        public DataHandler()
        {
            UpdateCache();
        }

        private void UpdateCache()
        {
            var actionSettings = Configuration.Plugin.Get(c => c.Actions);
            if (!Equals(_lastActionSettingsHash, actionSettings.GetHashCode()))
            {
                _lastActionSettingsHash = actionSettings.GetHashCode();
                _fuzzyDonations = Configuration.Plugin.Get(c => c.FuzzyDonations);
                _fuzzyBits = Configuration.Plugin.Get(c => c.FuzzyBits);
                _fuzzySubs = Configuration.Plugin.Get(c => c.FuzzySubs);
                _donationActions.Clear();
                _bitsActions.Clear();
                _twitchSubActions.Clear();
                _youtubeSubActions.Clear();
                _mixSubActions.Clear();
                _actions.Clear();
                _twitchHostActions.Clear();
                _twitchRaidActions.Clear();
                _mixerHostActions.Clear();

                SetAction(_meteorAction, actionSettings.MeteorShower);
                SetAction(_warheadAction, actionSettings.WarheadDrop);
            }
        }

        private void SetAction(IAction action, Configuration.PluginConfig.Action config)
        {
            action.Message = config.Message;
            foreach (var configEvent in config.Events)
            {
                var split = configEvent.Split('-');
                if (_fuzzyDonations || _fuzzyBits || _fuzzySubs)
                {
                    if (_fuzzyDonations && configEvent.StartsWith("Don-"))
                        _donationActions.Add(int.Parse(split[1]), action);
                    else if (_fuzzySubs && configEvent.StartsWith("TSub-"))
                        _twitchSubActions.Add(int.Parse(split[1]), action);
                    else if (_fuzzyBits && configEvent.StartsWith("TBits-"))
                        _bitsActions.Add(int.Parse(split[1]), action);
                    else if (_fuzzySubs && configEvent.StartsWith("YSponsor-"))
                        _youtubeSubActions.Add(int.Parse(split[1]), action);
                    else if (_fuzzySubs && configEvent.StartsWith("MSub-")) _mixSubActions.Add(int.Parse(split[1]), action);
                }

                if (configEvent.StartsWith("THost-"))
                    _twitchHostActions.Add(int.Parse(split[1]), action);
                else if (configEvent.StartsWith("TRaid"))
                    _twitchRaidActions.Add(int.Parse(split[1]), action);
                else if (configEvent.StartsWith("MHost-"))
                    _mixerHostActions.Add(int.Parse(split[1]), action);
                else
                    _actions.Add(configEvent, action);
            }
        }

        private IAction GetAction(string action)
        {
            UpdateCache();
            return _actions.GetValueOrDefault(action, null);
        }

        private IAction GetDonationAction(string key)
        {
            UpdateCache();
            return GetFuzzyAction(_donationActions, _fuzzyDonations, key);
        }

        private IAction GetBitsAction(string key)
        {
            UpdateCache();
            return GetFuzzyAction(_bitsActions, _fuzzyBits, key);
        }

        private IAction GetTwitchSubAction(string key)
        {
            UpdateCache();
            return GetFuzzyAction(_twitchSubActions, _fuzzySubs, key);
        }

        private IAction GetYoutubeSubAction(string key)
        {
            UpdateCache();
            return GetFuzzyAction(_youtubeSubActions, _fuzzySubs, key);
        }

        private IAction GetMixerSubAction(string key)
        {
            UpdateCache();
            return GetFuzzyAction(_mixSubActions, _fuzzySubs, key);
        }

        private IAction GetFuzzyAction(Dictionary<int, IAction> dictionary, bool fuzzy, string key)
        {
            if (!fuzzy) return _actions.GetValueOrDefault(key, null);

            var amount = Convert.ToInt32(key.Split('-')[1]);
            if (dictionary.TryGetValue(amount, out var action)) return action;

            foreach (var pair in dictionary.OrderBy(pair => pair.Key))
            {
                if (pair.Key >= amount) break;

                action = pair.Value;
            }

            return action;
        }

        private void SendMessage(string msg, bool alwaysSendMessage, IAction action)
        {
            SessionHandler.RunOnMainThread(() =>
            {
                Logger.WriteLine(msg);
                if (action == null && !alwaysSendMessage) return;

                if (MyMultiplayer.Static != null)
                    MyMultiplayer.Static.SendChatMessageScripted(msg, ChatChannel.GlobalScripted, 0, "[StreamEngineer]");
                else
                    MyHud.Chat.ShowMessageScripted("[StreamEngineer]", msg);
            });
        }

        private string GetMessage(IAction action)
        {
            return action?.Message == null ? "" : " " + action.Message;
        }

        public void OnDonation(string name, int amount, string formattedAmount)
        {
            var action = GetDonationAction($"Don-{amount}");
            action?.Execute();

            var messageEvent = Configuration.Plugin.Get(c => c.Events.Donation);
            SendMessage(string.Format(messageEvent.Message, name, formattedAmount) + GetMessage(action), messageEvent.AlwaysSendMessage, action);
        }

        public void OnTwitchSubscription(string name, int months, string tier, bool resub)
        {
            var action = GetTwitchSubAction($"TSub-{months}");
            action?.Execute();
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

            SendMessage(string.Format(msg, name, tierMsg, months) + GetMessage(action), messages.AlwaysSendMessage, action);
        }

        public void OnTwitchBits(string name, int amount)
        {
            var action = GetBitsAction($"TBits-{amount}");
            action?.Execute();

            var messageEvent = Configuration.Plugin.Get(c => c.Events.TwitchBits);
            SendMessage(string.Format(messageEvent.Message, name, amount) + GetMessage(action), messageEvent.AlwaysSendMessage, action);
        }

        public void OnTwitchFollow(string name)
        {
            var action = GetAction("TFollow");
            action?.Execute();

            var messageEvent = Configuration.Plugin.Get(c => c.Events.TwitchFollowed);
            SendMessage(string.Format(messageEvent.Message, name) + GetMessage(action), messageEvent.AlwaysSendMessage, action);
        }

        public void OnTwitchHost(string name, int viewers)
        {
            var messageEvent = Configuration.Plugin.Get(c => c.Events.TwitchHost);
            SendMessage(string.Format(messageEvent.Message, name, viewers) + GetMessage(null), messageEvent.AlwaysSendMessage, null);
        }

        public void OnTwitchRaid(string name, int viewers)
        {
            var messageEvent = Configuration.Plugin.Get(c => c.Events.TwitchRaid);
            SendMessage(string.Format(messageEvent.Message, name, viewers) + GetMessage(null), messageEvent.AlwaysSendMessage, null);
        }

        public void OnYoutubeSubscription(string name)
        {
            var action = GetAction("YSub");
            action?.Execute();

            var messageEvent = Configuration.Plugin.Get(c => c.Events.YoutubeSubscription);
            SendMessage(string.Format(messageEvent.Message, name) + GetMessage(action), messageEvent.AlwaysSendMessage, action);
        }

        public void OnYoutubeSponsor(string name, int months)
        {
            var action = GetYoutubeSubAction($"YSponsor-{months}");
            action?.Execute();

            var messages = Configuration.Plugin.Get(c => c.Events.YoutubeSponsor);
            var msg = months == 1 ? messages.NewMessage : messages.ResubMessage;
            SendMessage(string.Format(msg, name, months) + GetMessage(action), messages.AlwaysSendMessage, action);
        }

        public void OnYoutubeSuperchat(string name, int amount, string formatted)
        {
            var messageEvent = Configuration.Plugin.Get(c => c.Events.YoutubeSuperchat);
            SendMessage("Not implemented yet!", messageEvent.AlwaysSendMessage, null);
        }

        public void OnMixerSubscription(string name, int months)
        {
            var action = GetMixerSubAction($"MSub-{months}");
            action?.Execute();

            var messages = Configuration.Plugin.Get(c => c.Events.MixerSubscription);
            var msg = months == 1 ? messages.NewMessage : messages.ResubMessage;
            SendMessage(string.Format(msg, name, months) + GetMessage(action), messages.AlwaysSendMessage, action);
        }

        public void OnMixerFollow(string name)
        {
            var action = GetAction("MFollow");
            action?.Execute();

            var messageEvent = Configuration.Plugin.Get(c => c.Events.MixerFollowMessage);
            SendMessage(string.Format(messageEvent.Message, name) + GetMessage(action), messageEvent.AlwaysSendMessage, action);
        }

        public void OnMixerHost(string name, int viewers)
        {
            var messageEvent = Configuration.Plugin.Get(c => c.Events.MixerHostMessage);
            SendMessage("Not implemented yet!", messageEvent.AlwaysSendMessage, null);
        }

        public ILogger Logger => Plugin.Static.Logger;
    }
}