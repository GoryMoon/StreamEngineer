using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace GoryMoon.StreamEngineer.Data
{
    public class StreamlabsDataSource : SocketDataSource
    {
        public StreamlabsDataSource(BaseDataHandler dataHandler, IDataPlugin plugin) : base(dataHandler, plugin)
        {
        }

        protected override string[] Event => new[] { "event" };
        protected override string Name => "Streamlabs";
        protected override string Url => "wss://sockets.streamlabs.com";

        public override void Init(string token)
        {
            base.Init(new Dictionary<string, string>
            {
                { "token", token }
            });
        }

        protected override void OnSocketMessage(string s, JObject obj)
        {
            var type = obj.Value<string>("type");
            var account = obj.Value<string>("for");

            if (!obj.ContainsKey("message") || obj["message"].Type != JTokenType.Array) return;
            foreach (var message in obj["message"])
            {
                var name = message.Value<string>("name");
                switch (type)
                {
                    case "donation":
                    {
                        name = message.Value<string>("from");
                        var amount = (int)double.Parse(message.Value<string>("amount"));
                        var formatted = message.Value<string>("formatted_amount");

                        DataHandler.OnDonation(name, amount, formatted);
                        break;
                    }
                    case "subscription":
                    {
                        var months = message.Value<int>("months");
                        switch (account)
                        {
                            case "twitch_account":
                            {
                                var tier = message.Value<string>("sub_plan");
                                DataHandler.OnTwitchSubscription(name, months, tier, false);
                                break;
                            }
                            case "youtube_account":
                                DataHandler.OnYoutubeSponsor(name, months);
                                break;
                        }

                        break;
                    }
                    case "resub":
                    {
                        var months = message.Value<int>("months");
                        switch (account)
                        {
                            case "twitch_account":
                            {
                                var tier = message.Value<string>("sub_plan");
                                DataHandler.OnTwitchSubscription(name, months, tier, true);
                                break;
                            }
                            case "youtube_account":
                                DataHandler.OnYoutubeSponsor(name, months);
                                break;
                        }

                        break;
                    }
                    case "follow" when account == "twitch_account":
                        DataHandler.OnTwitchFollow(name);
                        break;
                    case "follow" when account == "youtube_account":
                        DataHandler.OnYoutubeSubscription(name);
                        break;
                    case "host":
                    {
                        var viewers = int.Parse(message.Value<string>("viewers"));
                        switch (account)
                        {
                            case "twitch_account":
                                DataHandler.OnTwitchHost(name, viewers);
                                break;
                        }

                        break;
                    }
                    case "bits":
                    {
                        var amount = int.Parse(message.Value<string>("amount"));
                        DataHandler.OnTwitchBits(name, amount);
                        break;
                    }
                    case "raid":
                    {
                        var amount = message.Value<int>("raiders");
                        DataHandler.OnTwitchRaid(name, amount);
                        break;
                    }
                    case "superchat":
                    {
                        var amount = int.Parse(message.Value<string>("amount"));
                        var formatted = (string)message["displayString"];
                        DataHandler.OnYoutubeSuperchat(name, amount, formatted);
                        break;
                    }
                }
            }
        }
    }
}