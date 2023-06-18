namespace GoryMoon.StreamEngineer
{
    public static class Sounds
    {
        public const string Action = "ArcNewItemImpact";
        public const string Snap = "ArcHudGPSNotification1";
        public const string Drone = "ArcHudGPSNotification1";

        public static bool IsSpecial(string sound)
        {
            return sound == Snap || sound == Drone;
        }

        public static bool IsRegular(string sound)
        {
            return sound == Action;
        }
        
    }
}