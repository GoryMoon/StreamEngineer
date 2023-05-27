using GoryMoon.StreamEngineer.Data;
using Newtonsoft.Json.Linq;

namespace StreamElementsLogger;

public class DummyDataHandler : BaseDataHandler
{
    public DummyDataHandler(IDataPlugin plugin) : base(plugin)
    {
    }

    public override void OnDonation(string name, int amount, string formatted)
    {
    }

    public override void OnTwitchSubscription(string name, int months, string tier, bool resub)
    {
    }

    public override void OnYoutubeSponsor(string name, int months)
    {
    }

    public override void OnTwitchFollow(string name)
    {
    }

    public override void OnYoutubeSubscription(string name)
    {
    }

    public override void OnTwitchHost(string name, int viewers)
    {
    }

    public override void OnTwitchBits(string name, int amount)
    {
    }

    public override void OnTwitchRaid(string name, int amount)
    {
    }

    public override void OnYoutubeSuperchat(string name, int amount, string formatted)
    {
    }

    public override void OnTwitchExtension(string name, int amount, string action, JToken settings)
    {
    }

    public override void OnTwitchChannelPoints(string name, string id)
    {
    }

    public override void OnIntegrationApp(JObject action)
    {
    }

    public override void Dispose()
    {
    }
}