using System.Collections.Generic;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO
{
    public interface IMasterDataProvider : 
        ICraftEssenceDataProvider,
        IItemDataProvider,
        IServantDataProvider,
        ISkillDataProvider,
        ITreasureDeviceDataProvider
    {
        public MstQuest? ResolveQuest(int questId);
        public MstBuff? ResolveBuffAndCache(int id, bool reload = false);
        public MstClass? ResolveClass(int classId, bool reload = false);
        
        public MstEvent GetEventById(int eventId);

        public IEnumerable<int> GetAttributeLists(bool reload = false);
    }
}