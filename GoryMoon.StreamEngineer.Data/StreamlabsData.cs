using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SocketIOClient;

namespace GoryMoon.StreamEngineer.Data
{
    public class StreamlabsData: IDisposable
    {
        private readonly BaseDataHandler _baseDataHandler;

        private bool _running;
        private SocketIO _socketIo;

        public StreamlabsData(BaseDataHandler baseDataHandler)
        {
            _baseDataHandler = baseDataHandler;
        }
        

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
            _socketIo.OnConnected += () => _baseDataHandler.Logger.WriteLine("Connected to Streamlabs");
            _socketIo.OnClosed += OnSocketClosed;
            _socketIo.On("event", args =>
            {
                try
                {
                    _baseDataHandler.Logger.WriteLine("Message: " + args.Text);
                    OnSocketMessage(args.Text);
                }
                catch (Exception e)
                {
                    _baseDataHandler.Logger.WriteLine(e);
                }
            });
            _socketIo.ConnectAsync();
            _running = true;
        }

        private async void OnSocketClosed(ServerCloseReason reason)
        {
            _baseDataHandler.Logger.WriteLine($"Closed Streamlabs: {reason}");
            if (reason != ServerCloseReason.ClosedByClient)
            {
                _baseDataHandler.Logger.WriteLine("Reconnecting Streamlabs");
                for (var i = 0; i < 3; i++)
                    try
                    {
                        await _socketIo.ConnectAsync();
                        return;
                    }
                    catch (WebSocketException ex)
                    {
                        _baseDataHandler.Logger.WriteLine(ex.Message);
                        await Task.Delay(2000);
                    }

                _baseDataHandler.Logger.WriteLine("Tried to reconnect 3 times, unable to connect to the server");
            }

            _running = false;
        }

        private void OnSocketMessage(string args)
        {
            _baseDataHandler.Logger.WriteLine("Socketmessage: " + args);
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
                        
                        _baseDataHandler.OnDonation(name, amount, formatted);
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
                                _baseDataHandler.OnTwitchSubscription(name, months, tier, false);
                                break;
                            }
                            case "youtube_account":
                                _baseDataHandler.OnYoutubeSponsor(name, months);
                                break;
                            case "mixer_account":
                                _baseDataHandler.OnMixerSubscription(name, months);
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
                                _baseDataHandler.OnTwitchSubscription(name, months, tier, true);
                                break;
                            }
                            case "youtube_account":
                                _baseDataHandler.OnYoutubeSponsor(name, months);
                                break;
                            case "mixer_account":
                                _baseDataHandler.OnMixerSubscription(name, months);
                                break;
                        }

                        break;
                    }
                    case "follow" when account == "twitch_account":
                        _baseDataHandler.OnTwitchFollow(name);
                        break;
                    case "follow" when account == "youtube_account":
                        _baseDataHandler.OnYoutubeSubscription(name);
                        break;
                    case "follow" when account == "mixer_account":
                    {
                        _baseDataHandler.OnMixerFollow(name);
                        break;
                    }
                    case "host":
                    {
                        var viewers = int.Parse((string) message["viewers"]);
                        switch (account)
                        {
                            case "twitch_account":
                                _baseDataHandler.OnTwitchHost(name, viewers);
                                break;
                            case "mixer_account":
                                _baseDataHandler.OnMixerHost(name, viewers);
                                break;
                        }

                        break;
                    }
                    case "bits":
                    {
                        var amount = int.Parse((string) message["amount"]);
                        _baseDataHandler.OnTwitchBits(name, amount);
                        break;
                    }
                    case "raid":
                    {
                        var amount = (int) message["raiders"];
                        _baseDataHandler.OnTwitchRaid(name, amount);
                        break;
                    }
                    case "superchat":
                    {
                        var amount = int.Parse((string) message["amount"]);
                        var formatted = (string) message["displayString"];
                        _baseDataHandler.OnYoutubeSuperchat(name, amount, formatted);
                        break;
                    }
                }
            }
        }
    }
}