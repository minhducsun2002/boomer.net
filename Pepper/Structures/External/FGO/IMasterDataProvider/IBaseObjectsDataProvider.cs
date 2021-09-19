using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO
{
    public interface IBaseObjectsDataProvider
    {
        public MstBuff? ResolveBuffAndCache(int id, bool reload = false);
        public MstClass? ResolveClass(int classId, bool reload = false);
    }
}