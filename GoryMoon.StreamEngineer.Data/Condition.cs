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

        public bool Test(Data data)
        {
            bool flag;
            if (!Type.Equals(data.Type)) return false;
            if (data.Amount == -1) return true;

            if (To > -1)
            {
                flag = To >= data.Amount;
            }
            else
            {
                flag = true;
            }

            if (From > -1)
            {
                flag &= From <= data.Amount;
            }

            return flag;
        }
        
        public override string ToString()
        {
            return $"{nameof(Type)}: {Type}, {nameof(To)}: {To}, {nameof(From)}: {From}";
        }
    }
}