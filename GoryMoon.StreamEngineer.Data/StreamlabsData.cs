using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SocketIOClient;

namespace GoryMoon.StreamEngineer.Data
{
    public class StreamlabsData: SocketData
    {
        protected override string Event => "event";
        protected override string Name => "Streamlabs";
        protected override string Url => "wss://sockets.streamlabs.com";
        
        public StreamlabsData(BaseDataHandler baseDataHandler): base(baseDataHandler) {}

        public void Init(string token)
        {
            base.Init(new Dictionary<string, string>
            {
                {"token", token}
            });
        }

        protected override void OnSocketMessage(string args)
        {
            var o = JObject.Parse(args);
            var type = (string) o["type"];
            var account = (string) o["for"];

            if (!o.ContainsKey("message") || o["message"].Type != JTokenType.Array) return;
            foreach (var message in o["message"])
            {
                var name = (string) message["name"];
                switch (type)
                {
                    case "donation":
                    {
                        name = (string) message["from"];
                        var amount = (int) double.Parse((string) message["amount"]);
                        var formatted = (string) message["formatted_amount"];
                        
                        BaseDataHandler.OnDonation(name, amount, formatted);
                        break;
                    }
                    case "subscription":
                    {
                        var months = (int) message["months"];
                        switch (account)
                        {
                            case "twitch_account":
                            {
                                var tier = (string) message["sub_plan"];
                                BaseDataHandler.OnTwitchSubscription(name, months, tier, false);
                                break;
                            }
                            case "youtube_account":
                                BaseDataHandler.OnYoutubeSponsor(name, months);
                                break;
                            case "mixer_account":
                                BaseDataHandler.OnMixerSubscription(name, months);
                                break;
                        }

                        break;
                    }
                    case "resub":
                    {
                        var months = (int) message["months"];
                        switch (account)
                        {
                            case "twitch_account":
                            {
                                var tier = (string) message["sub_plan"];
                                BaseDataHandler.OnTwitchSubscription(name, months, tier, true);
                                break;
                            }
                            case "youtube_account":
                                BaseDataHandler.OnYoutubeSponsor(name, months);
                                break;
                            case "mixer_account":
                                BaseDataHandler.OnMixerSubscription(name, months);
                                break;
                        }

                        break;
                    }
                    case "follow" when account == "twitch_account":
                        BaseDataHandler.OnTwitchFollow(name);
                        break;
                    case "follow" when account == "youtube_account":
                        BaseDataHandler.OnYoutubeSubscription(name);
                        break;
                    case "follow" when account == "mixer_account":
                    {
                        BaseDataHandler.OnMixerFollow(name);
                        break;
                    }
                    case "host":
                    {
                        var viewers = int.Parse((string) message["viewers"]);
                        switch (account)
                        {
                            case "twitch_account":
                                BaseDataHandler.OnTwitchHost(name, viewers);
                                break;
                            case "mixer_account":
                                BaseDataHandler.OnMixerHost(name, viewers);
                                break;
                        }

                        break;
                    }
                    case "bits":
                    {
                        var amount = int.Parse((string) message["amount"]);
                        BaseDataHandler.OnTwitchBits(name, amount);
                        break;
                    }
                    case "raid":
                    {
                        var amount = (int) message["raiders"];
                        BaseDataHandler.OnTwitchRaid(name, amount);
                        break;
                    }
                    case "superchat":
                    {
                        var amount = int.Parse((string) message["amount"]);
                        var formatted = (string) message["displayString"];
                        BaseDataHandler.OnYoutubeSuperchat(name, amount, formatted);
                        break;
                    }
                }
            }
        }
    }
}