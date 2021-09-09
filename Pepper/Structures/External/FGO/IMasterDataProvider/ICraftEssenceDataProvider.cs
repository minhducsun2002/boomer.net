using Pepper.Structures.External.FGO.Entities;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO
{
    public interface ICraftEssenceDataProvider
    {
        public CraftEssence? GetCraftEssenceById(int id, MstSvt? mstSvtHint = null);
        public CraftEssence? GetCraftEssenceByCollectionNo(int collectionNo, MstSvt? mstSvtHint = null);
        public MstSvt[] GetAllCraftEssenceEntities();
    }
}