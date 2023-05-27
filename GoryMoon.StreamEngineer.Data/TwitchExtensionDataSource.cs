using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace GoryMoon.StreamEngineer.Data
{
    public class TwitchExtensionDataSource : SocketDataSource
    {
        public TwitchExtensionDataSource(BaseDataHandler dataHandler, IDataPlugin plugin) : base(dataHandler,
            plugin)
        {
        }

        protected override string[] Event => new[] { "action", "cp_action" };
        protected override string Name => "Twitch Extension";
        protected override string Url => "wss://smapi.gorymoon.se/v2";

        public override void Init(string token)
        {
            base.Init(new Dictionary<string, string>
            {
                { "token", token },
                { "game", "spaceengineers" }
            });
        }

        protected override void OnSocketMessage(string s, JObject obj)
        {
            switch (s)
            {
                case "action":
                    DataHandler.OnTwitchExtension(obj.Value<string>("user"), obj.Value<int>("bits"),
                        obj.Value<string>("action"), obj["settings"]);
                    break;
                case "cp_action":
                    DataHandler.OnTwitchChannelPoints(obj.Value<string>("user"), obj.Value<string>("id"));
                    break;
            }
        }
    }
}