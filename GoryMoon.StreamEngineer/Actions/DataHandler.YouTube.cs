using GoryMoon.StreamEngineer.Config;
using GoryMoon.StreamEngineer.Data;

namespace GoryMoon.StreamEngineer.Actions
{
    public partial class DataHandler
    {
        public override void OnYoutubeSubscription(string name)
        {
            var actions = GetAndExecute(new Data.Data { Type = EventType.YoutubeSubscription, Amount = -1 });

            var messageEvent = Configuration.Plugin.Get(c => c.Events.YoutubeSubscription);
            SendMessage(string.Format(messageEvent.Message, name), messageEvent.AlwaysSendMessage, actions);
        }

        public override void OnYoutubeSponsor(string name, int months)
        {
            var actions = GetAndExecute(new Data.Data { Type = EventType.YoutubeSponsor, Amount = months });

            var messages = Configuration.Plugin.Get(c => c.Events.YoutubeSponsor);
            var msg = months == 1 ? messages.NewMessage : messages.ResubMessage;
            SendMessage(string.Format(msg, name, months), messages.AlwaysSendMessage, actions);
        }

        public override void OnYoutubeSuperchat(string name, int amount, string formatted)
        {
            var actions = GetAndExecute(new Data.Data { Type = EventType.YoutubeSuperchat, Amount = amount });
            var messageEvent = Configuration.Plugin.Get(c => c.Events.YoutubeSuperchat);
            SendMessage(string.Format(messageEvent.Message, name, formatted), messageEvent.AlwaysSendMessage, actions);
        }
    }
}