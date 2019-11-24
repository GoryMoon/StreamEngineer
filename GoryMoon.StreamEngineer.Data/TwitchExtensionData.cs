using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SocketIOClient;

namespace GoryMoon.StreamEngineer.Data
{
    public class TwitchExtensionData: SocketData
    {
        public TwitchExtensionData(BaseDataHandler baseDataHandler, IDataPlugin plugin) : base(baseDataHandler, plugin) {}

        protected override string Event => "action";
        protected override string Name => "Twitch Extension";
        protected override string Url => "wss://seapi.gorymoon.se/v1";

        public void Init(string token)
        {
            base.Init(new Dictionary<string, string>
            {
                {"token", token}
            });
        }

        protected override void OnSocketMessage(string args)
        {
            var data = JObject.Parse(args);
            BaseDataHandler.OnTwitchExtension((string) data["user"], (int) data["bits"], (string) data["action"], data["settings"]);
        }
    }
}