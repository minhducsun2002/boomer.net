using System.Collections.Generic;
using Pepper.Structures.External.FGO.Entities;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO
{
    public interface IMasterDataProvider
    {
        public BaseServant GetServant(MstSvt svt) => GetServant(svt.ID, svt);
        public BaseServant GetServant(int id, MstSvt? hint = null);
        public MstSvt? GetServantEntityById(int id);
        public MstSvt? GetServantEntityByCollectionNo(int collectionNo);
        public ServantLimits GetServantLimits(int servantId);

        public MstSvt[] GetAllServantEntities();

        public CraftEssence? GetCraftEssenceById(int id, MstSvt? mstSvtHint = null);
        public CraftEssence? GetCraftEssenceByCollectionNo(int collectionNo, MstSvt? mstSvtHint = null);
        
        public List<MstSvtTreasureDevice> GetCachedServantTreasureDevices(int servantId, bool reload = false);
        public TreasureDevice GetTreasureDevice(int treasureDeviceId);
        public MstTreasureDevice? GetTreasureDeviceEntity(int treasureDeviceId);
        public MstTreasureDeviceLv GetNPGain(int svtId);

        public Skill? GetSkillById(int id, MstSkill? mstSkillHint = null);
        public MstSkill[] GetSkillEntityByActIndividuality(int individuality);

        public MstSvtSkill[] GetServantSkillAssociationBySkillId(int skillId);
        public MstSvtSkill[] GetServantSkillAssociationByServantId(int svtId);
        
        public MstQuest? ResolveQuest(int questId);
        public MstBuff? ResolveBuffAndCache(int id, bool reload = false);
        public MstClass? ResolveClass(int classId, bool reload = false);
        
        public MstEvent GetEventById(int eventId);
        
        public MstItem[] GetItemsByIndividuality(int individualty);
        public string? GetItemName(int itemId, bool reload = false);
        
        public IEnumerable<int> GetAttributeLists(bool reload = false);
    }
}