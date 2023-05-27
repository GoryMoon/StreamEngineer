using GoryMoon.StreamEngineer.Config;
using GoryMoon.StreamEngineer.Data;

namespace GoryMoon.StreamEngineer.Actions
{
    public partial class DataHandler
    {
        public override void OnTwitchSubscription(string name, int months, string tier, bool resub)
        {
            var actions = GetAndExecute(new Data.Data
            {
                Type = EventType.TwitchSubscription, Amount = months, Tier = tier == "2000" ? 2 : tier == "3000" ? 3 : 1
            });
            var messages = Configuration.Plugin.Get(c => c.Events.TwitchSubscription);

            var msg = months == 1 ? messages.NewMessage : messages.ResubMessage;
            string tierMsg;
            switch (tier)
            {
                case "2000":
                    tierMsg = messages.Tier2;
                    break;
                case "3000":
                    tierMsg = messages.Tier3;
                    break;
                default:
                    tierMsg = messages.Tier1;
                    break;
            }

            SendMessage(string.Format(msg, name, tierMsg, months), messages.AlwaysSendMessage, actions);
        }

        public override void OnTwitchBits(string name, int amount)
        {
            var actions = GetAndExecute(new Data.Data { Type = EventType.TwitchBits, Amount = amount });

            var messageEvent = Configuration.Plugin.Get(c => c.Events.TwitchBits);
            SendMessage(string.Format(messageEvent.Message, name, amount), messageEvent.AlwaysSendMessage, actions);
        }

        public override void OnTwitchFollow(string name)
        {
            var actions = GetAndExecute(new Data.Data { Type = EventType.TwitchFollow, Amount = -1 });

            var messageEvent = Configuration.Plugin.Get(c => c.Events.TwitchFollowed);
            SendMessage(string.Format(messageEvent.Message, name), messageEvent.AlwaysSendMessage, actions);
        }

        public override void OnTwitchHost(string name, int viewers)
        {
            var actions = GetAndExecute(new Data.Data { Type = EventType.TwitchHost, Amount = viewers });
            var messageEvent = Configuration.Plugin.Get(c => c.Events.TwitchHost);
            SendMessage(string.Format(messageEvent.Message, name, viewers), messageEvent.AlwaysSendMessage, actions);
        }

        public override void OnTwitchRaid(string name, int viewers)
        {
            var actions = GetAndExecute(new Data.Data { Type = EventType.TwitchRaid, Amount = viewers });
            var messageEvent = Configuration.Plugin.Get(c => c.Events.TwitchRaid);
            SendMessage(string.Format(messageEvent.Message, name, viewers), messageEvent.AlwaysSendMessage, actions);
        }
    }
}