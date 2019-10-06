using System;
using System.IO;

namespace GoryMoon.StreamEngineer.Data
{
    public class Testing
    {
        public static void Main()
        {
            var dataHandler = new TestBaseDataHandler();
            var streamlabsData = new StreamlabsData(dataHandler);
            var token = Environment.GetEnvironmentVariable("token");
            streamlabsData.Init(token);
            //var data = new Data {Type = EventType.TwitchBits, Amount = 10000};
            //var actions = handler.GetActions(data);
            //actions.ForEach(action => action.Execute());

            Console.WriteLine("Stop with: q");
            while (true)
            {
                var text = Console.ReadLine();
                if ("q".Equals(text))
                {
                    streamlabsData.Dispose();
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
            private ActionHandler _handler;
            
            public TestBaseDataHandler() : base(new TestLogger())
            {
                var path = Path.GetDirectoryName(
                    Uri.UnescapeDataString(new UriBuilder(typeof(Testing).Assembly.CodeBase).Path));
                _handler = new ActionHandler(path, "events.json", new TestLogger());
                _handler.AddAction("test", typeof(TestAction));
                _handler.StartWatching();
            }

            public override void OnDonation(string name, int amount, string formatted)
            {
                Logger.WriteLine("OnDonation");
                var data = new Data() {Type = EventType.Donation, Amount = amount};
                _handler.GetActions(data).ForEach(action => action.Execute(data));
            }

            public override void OnTwitchSubscription(string name, int months, string tier, bool resub)
            {
                Logger.WriteLine("OnTwitchSubscription");
                var data = new Data() {Type = EventType.MixerSubscription, Amount = months};
                _handler.GetActions(data).ForEach(action => action.Execute(data));
            }

            public override void OnYoutubeSponsor(string name, int months)
            {
                Logger.WriteLine("OnYoutubeSponsor");
                var data = new Data() {Type = EventType.YoutubeSponsor, Amount = months};
                _handler.GetActions(data).ForEach(action => action.Execute(data));
            }

            public override void OnMixerSubscription(string name, int months)
            {
                Logger.WriteLine("OnMixerSubscription");
                var data = new Data() {Type = EventType.MixerSubscription, Amount = months};
                _handler.GetActions(data).ForEach(action => action.Execute(data));
            }

            public override void OnTwitchFollow(string name)
            {
                Logger.WriteLine("OnTwitchFollow");
                var data = new Data() {Type = EventType.TwitchFollow, Amount = -1};
                _handler.GetActions(data).ForEach(action => action.Execute(data));
            }

            public override void OnYoutubeSubscription(string name)
            {
                Logger.WriteLine("OnYoutubeSubscription");
                var data = new Data() {Type = EventType.YoutubeSubscription, Amount = -1};
                _handler.GetActions(data).ForEach(action => action.Execute(data));
            }

            public override void OnMixerFollow(string name)
            {
                Logger.WriteLine("OnMixerFollow");
                var data = new Data() {Type = EventType.MixerFollow, Amount = -1};
                _handler.GetActions(data).ForEach(action => action.Execute(data));
            }

            public override void OnTwitchHost(string name, int viewers)
            {
                Logger.WriteLine("OnTwitchHost");
                var data = new Data() {Type = EventType.TwitchHost, Amount = viewers};
                _handler.GetActions(data).ForEach(action => action.Execute(data));
            }

            public override void OnMixerHost(string name, int viewers)
            {
                Logger.WriteLine("OnMixerHost");
                var data = new Data() {Type = EventType.MixerHost, Amount = viewers};
                _handler.GetActions(data).ForEach(action => action.Execute(data));
            }

            public override void OnTwitchBits(string name, int amount)
            {
                Logger.WriteLine("OnTwitchBits");
                var data = new Data() {Type = EventType.TwitchBits, Amount = amount};
                _handler.GetActions(data).ForEach(action => action.Execute(data));
            }

            public override void OnTwitchRaid(string name, int amount)
            {
                Logger.WriteLine("OnTwitchRaid");
                var data = new Data() {Type = EventType.TwitchRaid, Amount = amount};
                _handler.GetActions(data).ForEach(action => action.Execute(data));
            }

            public override void OnYoutubeSuperchat(string name, int amount, string formatted)
            {
                Logger.WriteLine("OnYoutubeSuperchat");
                var data = new Data() {Type = EventType.YoutubeSuperchat, Amount = amount};
                _handler.GetActions(data).ForEach(action => action.Execute(data));
            }

            public override void Dispose()
            {
                _handler.Dispose();
            }
        }

        private class TestAction: BaseAction
        {
        
            public int Radius { get; set; }
            public int Amount { get; set; }
        
            public override void Execute(Data data)
            {
                Console.WriteLine(ToString());
            }

            public override string ToString()
            {
                return base.ToString() + $" {nameof(Radius)}: {Radius}, {nameof(Amount)}: {Amount}";
            }
        }
    }
}