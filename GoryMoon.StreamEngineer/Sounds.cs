using GoryMoon.StreamEngineer.Config;

namespace GoryMoon.StreamEngineer
{
    public static class Sounds
    {
        public const string Action = "ArcNewItemImpact";
        public const string Snap = "ArcHudGPSNotification1";
        public const string Drone = "ArcHudGPSNotification1";

        public static bool GetSound(string action, out string sound)
        {
            var sounds = Configuration.Plugin.Get(config => config.ActionSound);
            return sounds.TryGetValue(action, out sound) && sound != "";
        }
        
    }
}