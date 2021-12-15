using Pepper.Commons.Osu;

namespace Pepper.Structures.External.Osu.Extensions
{
    public static class GameServerExtensions
    {
        public static string GetDisplayText(this GameServer server)
        {
            return server switch
            {
                GameServer.Osu => "osu!",
                GameServer.Ripple => "Ripple",
                _ => "default"
            };
        }
    }
}