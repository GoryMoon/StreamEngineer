using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GoryMoon.StreamEngineer.Data
{
    public class Testing
    {
        public static void Main()
        {
            var dataHandler = new TestBaseDataHandler();
            var streamlabsData = new StreamlabsData(dataHandler);
            var twitchExtData = new TwitchExtensionData(dataHandler);
            var token = Environment.GetEnvironmentVariable("token");
            var twitchToken = Environment.GetEnvironmentVariable("twitch_token");
            streamlabsData.Init(token);
            twitchExtData.Init(twitchToken);

            Console.WriteLine("Stop with: q");
            while (true)
            {
                var text = Console.ReadLine();
                if ("q".Equals(text))
                {
                    streamlabsData.Dispose();
                    twitchExtData.Dispose();
                    return;
                }
            }
        }

        private class TestLogger : ILogger
        {
            public void WriteLine(string msg)
            {
                Console.WriteLine(msg);
            }

            public void WriteLine(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private class TestBaseDataHandler : BaseDataHandler
        {
            private readonly ActionHandler _handler;

            public TestBaseDataHandler() : base(new TestLogger())
            {
                var path = Path.GetDirectoryName(
                    Uri.UnescapeDataString(new UriBuilder(typeof(Testing).Assembly.CodeBase).Path));
                _handler = new ActionHandler(path, "events.json", new TestLogger());
                _handler.AddAction("meteors", typeof(TestAction));
                _handler.AddAction("random", typeof(RandomAction));
                _handler.StartWatching();
            }

            public override void OnDonation(string name, int amount, string formatted)
            {
                Logger.WriteLine("OnDonation");
                var data = new Data {Type = EventType.Donation, Amount = amount};
                _handler.GetActions(data).ForEach(action => action.Execute(data));
            }

            public override void OnTwitchSubscription(string name, int months, string tier, bool resub)
            {
                Logger.WriteLine("OnTwitchSubscription");
                var data = new Data {Type = EventType.MixerSubscription, Amount = months};
                _handler.GetActions(data).ForEach(action => action.Execute(data));
            }

            public override void OnYoutubeSponsor(string name, int months)
            {
                Logger.WriteLine("OnYoutubeSponsor");
                var data = new Data {Type = EventType.YoutubeSponsor, Amount = months};
                _handler.GetActions(data).ForEach(action => action.Execute(data));
            }

            public override void OnMixerSubscription(string name, int months)
            {
                Logger.WriteLine("OnMixerSubscription");
                var data = new Data {Type = EventType.MixerSubscription, Amount = months};
                _handler.GetActions(data).ForEach(action => action.Execute(data));
            }

            public override void OnTwitchFollow(string name)
            {
                Logger.WriteLine("OnTwitchFollow");
                var data = new Data {Type = EventType.TwitchFollow, Amount = -1};
                _handler.GetActions(data).ForEach(action => action.Execute(data));
            }

            public override void OnYoutubeSubscription(string name)
            {
                Logger.WriteLine("OnYoutubeSubscription");
                var data = new Data {Type = EventType.YoutubeSubscription, Amount = -1};
                _handler.GetActions(data).ForEach(action => action.Execute(data));
            }

            public override void OnMixerFollow(string name)
            {
                Logger.WriteLine("OnMixerFollow");
                var data = new Data {Type = EventType.MixerFollow, Amount = -1};
                _handler.GetActions(data).ForEach(action => action.Execute(data));
            }

            public override void OnTwitchHost(string name, int viewers)
            {
                Logger.WriteLine("OnTwitchHost");
                var data = new Data {Type = EventType.TwitchHost, Amount = viewers};
                _handler.GetActions(data).ForEach(action => action.Execute(data));
            }

            public override void OnMixerHost(string name, int viewers)
            {
                Logger.WriteLine("OnMixerHost");
                var data = new Data {Type = EventType.MixerHost, Amount = viewers};
                _handler.GetActions(data).ForEach(action => action.Execute(data));
            }

            public override void OnTwitchBits(string name, int amount)
            {
                Logger.WriteLine("OnTwitchBits");
                var data = new Data {Type = EventType.TwitchBits, Amount = amount};
                _handler.GetActions(data).ForEach(action => action.Execute(data));
            }

            public override void OnTwitchRaid(string name, int amount)
            {
                Logger.WriteLine("OnTwitchRaid");
                var data = new Data {Type = EventType.TwitchRaid, Amount = amount};
                _handler.GetActions(data).ForEach(action => action.Execute(data));
            }

            public override void OnYoutubeSuperchat(string name, int amount, string formatted)
            {
                Logger.WriteLine("OnYoutubeSuperchat");
                var data = new Data {Type = EventType.YoutubeSuperchat, Amount = amount};
                _handler.GetActions(data).ForEach(action => action.Execute(data));
            }

            public override void OnTwitchExtension(string name, int amount, string action, JToken settings)
            {
                Logger.WriteLine("OnTwitchExtension");
                if (_handler.GetAction(action, settings, out var baseAction))
                {
                    baseAction.Execute(new Data {Type = EventType.TwitchExtension, Amount = amount});
                }
            }

            public override void Dispose()
            {
                _handler.Dispose();
            }
        }

        private class TestAction : BaseAction
        {
            public double Radius { get; set; }
            public string Amount { get; set; }

            public override void Execute(Data data)
            {
                Console.WriteLine(ToString());
                Console.WriteLine(GetEventValue(Amount, 1, data));
            }

            public override string ToString()
            {
                return base.ToString() + $" {nameof(Radius)}: {Radius}, {nameof(Amount)}: {Amount}";
            }
        }

        [JsonObject(MemberSerialization.OptIn)]
        private class RandomAction : BaseAction
        {
            private DynamicRandomSelector<BaseAction> _actions;

            [JsonExtensionData] private IDictionary<string, JToken> _additionalData;

            private Random _random = new Random();

            [OnDeserialized]
            private void OnDeserialized(StreamingContext context)
            {
                if (!(context.Context is ActionHandler handler)) return;
                var actions = _additionalData["actions"].ToObject<List<JToken>>();
                _actions = new DynamicRandomSelector<BaseAction>();
                Message = "No action to randomize from";
                foreach (var action in actions)
                    if (handler.GetAction((string) action["type"], action["action"], out var baseAction))
                    {
                        var weight = (float) (action["weight"] ?? 1.0F);
                        _actions.Add(baseAction, weight);
                    }

                if (_actions.Count > 0)
                    _actions.Build();
            }

            public override void Execute(Data data)
            {
                if (_actions.Count <= 0) return;

                var action = _actions.SelectRandomItem();
                Message = action.Message;
                action.Execute(data);
            }
        }
    }
}