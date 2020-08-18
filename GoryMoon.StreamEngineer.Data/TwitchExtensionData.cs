using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace GoryMoon.StreamEngineer.Data
{
    public class TwitchExtensionData: SocketData
    {
        public TwitchExtensionData(BaseDataHandler baseDataHandler, IDataPlugin plugin) : base(baseDataHandler, plugin) {}

        protected override string[] Event => new []{"action", "cp_action"};
        protected override string Name => "Twitch Extension";
        protected override string Url => "wss://smapi.gorymoon.se/v2";

        public void Init(string token, string game = "spaceengineers")
        {
            base.Init(new Dictionary<string, string>
            {
                {"token", token},
                {"game", game}
            });
        }

        protected override void OnSocketMessage(string s, JObject obj)
        {
            switch (s)
            {
                case "action":
                    BaseDataHandler.OnTwitchExtension((string) obj["user"], (int) obj["bits"], (string) obj["action"], obj["settings"]);
                    break;
                case "cp_action":
                    BaseDataHandler.OnTwitchChannelPoints((string) obj["user"], (string) obj["id"]);
                    break;
            }
        }
    }
}