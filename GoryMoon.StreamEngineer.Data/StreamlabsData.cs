using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SocketIOClient;

namespace GoryMoon.StreamEngineer.Data
{
    public class StreamlabsData
    {
        private readonly IDataHandler _dataHandler;

        private bool _running;
        private SocketIO _socketIo;

        public StreamlabsData(IDataHandler dataHandler)
        {
            Static = this;
            _dataHandler = dataHandler;
        }

        private static StreamlabsData Static { get; set; }

        public void Dispose()
        {
            if (_running)
            {
                _socketIo?.CloseAsync();
                _running = false;
            }
        }

        public void Init(string token)
        {
            _socketIo = new SocketIO("wss://sockets.streamlabs.com")
            {
                Parameters = new Dictionary<string, string>
                {
                    {"token", token}
                }
            };
            _socketIo.OnConnected += () => _dataHandler.Logger.WriteLine("Connected to Streamlabs");
            _socketIo.OnClosed += OnSocketClosed;
            _socketIo.On("event", args =>
            {
                try
                {
                    _dataHandler.Logger.WriteLine("Message: " + args.Text);
                    Static.OnSocketMessage(args.Text);
                }
                catch (Exception e)
                {
                    _dataHandler.Logger.WriteLine(e);
                }
            });
            _socketIo.ConnectAsync();
            _running = true;
        }

        private async void OnSocketClosed(ServerCloseReason reason)
        {
            _dataHandler.Logger.WriteLine($"Closed Streamlabs: {reason}");
            if (reason != ServerCloseReason.ClosedByClient)
            {
                _dataHandler.Logger.WriteLine("Reconnecting Streamlabs");
                for (var i = 0; i < 3; i++)
                    try
                    {
                        await _socketIo.ConnectAsync();
                        return;
                    }
                    catch (WebSocketException ex)
                    {
                        _dataHandler.Logger.WriteLine(ex.Message);
                        await Task.Delay(2000);
                    }

                _dataHandler.Logger.WriteLine("Tried to reconnect 3 times, unable to connect to the server");
            }

            _running = false;
        }

        private void OnSocketMessage(string args)
        {
            _dataHandler.Logger.WriteLine("Socketmessage: " + args);
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
                        _dataHandler.OnDonation(name, amount, formatted);
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
                                _dataHandler.OnTwitchSubscription(name, months, tier, false);
                                break;
                            }
                            case "youtube_account":
                                _dataHandler.OnYoutubeSponsor(name, months);
                                break;
                            case "mixer_account":
                                _dataHandler.OnMixerSubscription(name, months);
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
                                _dataHandler.OnTwitchSubscription(name, months, tier, true);
                                break;
                            }
                            case "youtube_account":
                                _dataHandler.OnYoutubeSponsor(name, months);
                                break;
                            case "mixer_account":
                                _dataHandler.OnMixerSubscription(name, months);
                                break;
                        }

                        break;
                    }
                    case "follow" when account == "twitch_account":
                        _dataHandler.OnTwitchFollow(name);
                        break;
                    case "follow" when account == "youtube_account":
                        _dataHandler.OnYoutubeSubscription(name);
                        break;
                    case "follow" when account == "mixer_account":
                    {
                        _dataHandler.OnMixerFollow(name);
                        break;
                    }
                    case "host":
                    {
                        var viewers = int.Parse((string) message["viewers"]);
                        switch (account)
                        {
                            case "twitch_account":
                                _dataHandler.OnTwitchHost(name, viewers);
                                break;
                            case "mixer_account":
                                _dataHandler.OnMixerHost(name, viewers);
                                break;
                        }

                        break;
                    }
                    case "bits":
                    {
                        var amount = int.Parse((string) message["amount"]);
                        _dataHandler.OnTwitchBits(name, amount);
                        break;
                    }
                    case "raid":
                    {
                        var amount = (int) message["raiders"];
                        _dataHandler.OnTwitchRaid(name, amount);
                        break;
                    }
                    case "superchat":
                    {
                        var amount = int.Parse((string) message["amount"]);
                        var formatted = (string) message["displayString"];
                        _dataHandler.OnYoutubeSuperchat(name, amount, formatted);
                        break;
                    }
                }
            }
        }
    }
}