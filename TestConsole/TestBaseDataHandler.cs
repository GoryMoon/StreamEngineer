using System;
using System.IO;
using GoryMoon.StreamEngineer.Data;
using Newtonsoft.Json.Linq;

namespace TestConsole;

internal class TestBaseDataHandler : BaseDataHandler
{
    private readonly ActionHandler _handler;

    public TestBaseDataHandler(IDataPlugin plugin) : base(plugin)
    {
        var path = Path.GetDirectoryName(
            Uri.UnescapeDataString(new UriBuilder(typeof(Program).Assembly.Location).Path));
        _handler = new ActionHandler(path, "events.json", new TestLogger());
        _handler.AddAction(typeof(TestAction));
        _handler.AddAction(typeof(RandomAction));
        _handler.AddAction(typeof(SnapAction));
        _handler.PrintActionTypes();
        _handler.StartWatching();
    }

    public override void OnDonation(string name, int amount, string formatted)
    {
        Plugin.Logger.WriteLine("OnDonation");
        var data = new Data { Type = EventType.Donation, Amount = amount };
        _handler.GetActions(data).ForEach(action => action.Execute(data, GetParams(data)));
    }

    public override void OnTwitchSubscription(string name, int months, string tier, bool resub)
    {
        Plugin.Logger.WriteLine("OnTwitchSubscription");
        var data = new Data { Type = EventType.TwitchSubscription, Amount = months };
        _handler.GetActions(data).ForEach(action => action.Execute(data, GetParams(data)));
    }

    public override void OnYoutubeSponsor(string name, int months)
    {
        Plugin.Logger.WriteLine("OnYoutubeSponsor");
        var data = new Data { Type = EventType.YoutubeSponsor, Amount = months };
        _handler.GetActions(data).ForEach(action => action.Execute(data, GetParams(data)));
    }

    public override void OnTwitchFollow(string name)
    {
        Plugin.Logger.WriteLine("OnTwitchFollow");
        var data = new Data { Type = EventType.TwitchFollow, Amount = -1 };
        _handler.GetActions(data).ForEach(action => action.Execute(data, GetParams(data)));
    }

    public override void OnYoutubeSubscription(string name)
    {
        Plugin.Logger.WriteLine("OnYoutubeSubscription");
        var data = new Data { Type = EventType.YoutubeSubscription, Amount = -1 };
        _handler.GetActions(data).ForEach(action => action.Execute(data, GetParams(data)));
    }

    public override void OnTwitchHost(string name, int viewers)
    {
        Plugin.Logger.WriteLine("OnTwitchHost");
        var data = new Data { Type = EventType.TwitchHost, Amount = viewers };
        _handler.GetActions(data).ForEach(action => action.Execute(data, GetParams(data)));
    }

    public override void OnTwitchBits(string name, int amount)
    {
        Plugin.Logger.WriteLine("OnTwitchBits");
        var data = new Data { Type = EventType.TwitchBits, Amount = amount };
        _handler.GetActions(data).ForEach(action => action.Execute(data, GetParams(data)));
    }

    public override void OnTwitchRaid(string name, int amount)
    {
        Plugin.Logger.WriteLine("OnTwitchRaid");
        var data = new Data { Type = EventType.TwitchRaid, Amount = amount };
        _handler.GetActions(data).ForEach(action => action.Execute(data, GetParams(data)));
    }

    public override void OnYoutubeSuperchat(string name, int amount, string formatted)
    {
        Plugin.Logger.WriteLine("OnYoutubeSuperchat");
        var data = new Data { Type = EventType.YoutubeSuperchat, Amount = amount };
        _handler.GetActions(data).ForEach(action => action.Execute(data, GetParams(data)));
    }

    public override void OnTwitchExtension(string name, int amount, string action, JToken settings)
    {
        Plugin.Logger.WriteLine("OnTwitchExtension");
        if (_handler.GetAction(action, settings, out var baseAction))
        {
            var data = new Data { Type = EventType.TwitchExtension, Amount = amount };
            baseAction.Execute(data, GetParams(data));
        }
    }

    public override void OnTwitchChannelPoints(string name, string id)
    {
        Plugin.Logger.WriteLine("OnTwitchChannelPoints");
        var data = new Data { Type = EventType.TwitchChannelPoints, Amount = 0 };
        _handler.GetActions(data).ForEach(action => action.Execute(data, GetParams(data)));
    }

    public override void OnIntegrationApp(JObject action)
    {
        Plugin.Logger.WriteLine("OnIntegrationApp");
        var type = action.Value<string>("type") ?? string.Empty;
        var settings = action.GetValue("values");

        if (_handler.GetAction(type, settings, out var baseAction))
        {
            var data = new Data { Type = EventType.IntegrationApp, Amount = -1 };
            baseAction.Execute(data, GetParams(data));
        }

    }

    public override void Dispose()
    {
        _handler.Dispose();
    }
}