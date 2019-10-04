using GoryMoon.StreamEngineer.Config;
using Sandbox.Game.World;

namespace GoryMoon.StreamEngineer
{
    public class Utils
    {

        public static MyPlayer GetPlayer()
        {
            return MySession.Static.Players.GetPlayerByName(Configuration.Plugin.Get(c => c.SteamName));
        }
        
    }
}