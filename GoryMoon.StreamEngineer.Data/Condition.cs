using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace GoryMoon.StreamEngineer.Data
{
    public class Condition
    {
        [JsonConverter(typeof(StringEnumConverter), typeof(SnakeCaseNamingStrategy))]
        public EventType Type { get; set; }
        public double From { get; set; } = -1;
        public double To { get; set; } = -1;
        [JsonProperty("channel_point_id")]
        public string Id { get; set; } = "";

        public bool Test(Data data)
        {
            if (Type.Equals(EventType.TwitchSubscriptionTier) && EventType.TwitchSubscription.Equals(data.Type))
            {
                return Test(data.Tier);
            }

            if (Type.Equals(EventType.TwitchChannelPoints) && EventType.TwitchChannelPoints.Equals(data.Type))
            {
                return Id.Equals(data.Id);
            }

            if (!Type.Equals(data.Type)) return false;
            return data.Amount == -1 || Test(data.Amount);
        }

        private bool Test(int val)
        {
            bool flag;
            if (To > -1)
            {
                flag = To >= val;
            }
            else
            {
                flag = true;
            }

            if (From > -1)
            {
                flag &= From <= val;
            }
            return flag;
        }
        
        public override string ToString()
        {
            return $"{nameof(Type)}: {Type}, {nameof(To)}: {To}, {nameof(From)}: {From}";
        }
    }
}