namespace Pepper.Commons.Osu
{
    internal class GameServerAttribute : System.Attribute
    {
        public readonly GameServer Server;
        public GameServerAttribute(GameServer gameServer)
        {
            Server = gameServer;
        }
    }
}