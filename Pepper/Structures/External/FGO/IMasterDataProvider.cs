using System.Collections.Generic;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO
{
    public interface IMasterDataProvider : 
        ICraftEssenceDataProvider,
        IItemDataProvider,
        IServantDataProvider,
        ISkillDataProvider,
        ITreasureDeviceDataProvider,
        IQuestDataProvider,
        IBaseObjectsDataProvider,
        IMapDataProvider
    {
        public IEnumerable<int> GetAttributeLists(bool reload = false);
    }
}