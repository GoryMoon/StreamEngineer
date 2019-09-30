using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Sandbox.Engine.Multiplayer;
using SocketIOClient;
using VRage.Network;

namespace GoryMoon.StreamEngineer.Data
{
    public class StreamlabsData
    {
        private SocketIO _socketIo;
        private readonly DataHandler _dataHandler;

        private static StreamlabsData Static { get; set; }
        public StreamlabsData(DataHandler dataHandler)
        {
            Static = this;
            _dataHandler = dataHandler;
        }

        public void Dispose()
        {
            _socketIo?.CloseAsync();
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
            _socketIo.OnConnected += () => Logger.WriteLine("Connected to Streamlabs");
            _socketIo.OnClosed += OnSocketClosed;
            _socketIo.On("event", args =>
            {
                try
                {
                    Logger.WriteLine("Message: " + args.Text);
                    //MyMultiplayer.RaiseStaticEvent(x => OnSocketMessageEvent, args.Text);
                    Static.OnSocketMessage(args.Text);
                }
                catch (Exception e)
                {
                    Logger.WriteLine(e);
                }
            });
            _socketIo.ConnectAsync();
        }

        private async void OnSocketClosed(ServerCloseReason reason)
        {
            Logger.WriteLine($"Closed Streamlabs: {reason}");
            if (reason != ServerCloseReason.ClosedByClient)
            {
                Logger.WriteLine("Reconnecting Streamlabs");
                for (var i = 0; i < 3; i++)
                {
                    try
                    {
                        await _socketIo.ConnectAsync();
                        break;
                    }
                    catch (WebSocketException ex)
                    {
                        Logger.WriteLine(ex.Message);
                        await Task.Delay(2000);
                    }
                }

                Logger.WriteLine("Tried to reconnect 3 times, unable to connect to the server");
            }
        }

        /*[Event(null, 452)]
        [Reliable]
        [Server]*/
        private static void OnSocketMessageEvent(string args)
        {
            try
            {
                /*if (Sync.IsValidEventOnServer && (!MyEventContext.Current.IsLocallyInvoked && !MySession.Static.IsUserSpaceMaster(MyEventContext.Current.Sender.Value)))
                {
                    (MyMultiplayer.Static as MyMultiplayerServerBase)?.ValidationFailed(MyEventContext.Current.Sender.Value);
                    MyEventContext.ValidationFailed();
                    Logger.WriteLine("Invalid message: " + args);
                }
                else
                {*/
                    Static.OnSocketMessage(args);
                //}
            }
            catch (Exception e)
            {
                Logger.WriteLine(e);
            }
        }

        private void OnSocketMessage(string args)
        {
            Logger.WriteLine("Socketmessage: " + args);
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