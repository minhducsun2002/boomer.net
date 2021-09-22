using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO
{
    public interface IMapDataProvider
    {
        public MstSpot? ResolveSpot(int spotId, bool reload = false);
        public MstWar? ResolveWar(int warId, bool reload = false);
    }
}