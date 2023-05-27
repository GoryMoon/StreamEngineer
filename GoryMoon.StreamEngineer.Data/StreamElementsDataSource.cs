using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SocketIOClient;
using SocketIOClient.Transport;

namespace GoryMoon.StreamEngineer.Data
{
    public class StreamElementsDataSource : SocketDataSource
    {
        private string _token;

        public StreamElementsDataSource(BaseDataHandler dataHandler, IDataPlugin plugin) : base(dataHandler,
            plugin)
        {
        }

        protected override string[] Event => new[] { "authenticated", "unauthorized", "event", "event:test" };
        protected override string Name => "StreamElements";
        protected override string Url => "https://realtime.streamelements.com";

        public override void Init(string token)
        {
            _token = token;
            base.Init(new Dictionary<string, string>(), EngineIO.V3, TransportProtocol.WebSocket);
        }

        protected override void OnSocketMessage(string s, JObject obj)
        {
            switch (s)
            {
                case "authenticated":
                    Plugin.Logger.WriteAndChat($"[{Name}] Authenticated");
                    break;
                case "unauthorized":
                    Plugin.ConnectionError(Name, "Invalid JWT token");
                    break;
                case "event":

                    break;
            }
        }

        protected override async void OnConnected(SocketIO socket)
        {
            base.OnConnected(socket);
            var msg = new JObject
            {
                ["method"] = "jwt",
                ["token"] = _token
            };
            await socket.EmitAsync("authenticate", msg);
        }
    }
}