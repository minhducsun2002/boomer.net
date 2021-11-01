using MongoDB.Driver;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO
{
    public partial class MasterDataMongoDBConnection : IMasterDataProvider
    {
        // The field names must match the class names (i.e. a field named MstCollection1 must be of type IMongoCollection<MstCollection1>)
        private readonly IMongoCollection<MstSvt> MstSvt = null!;
        private readonly IMongoCollection<MstSvtLimit> MstSvtLimit = null!;
        private readonly IMongoCollection<MstSvtSkill> MstSvtSkill = null!;
        private readonly IMongoCollection<MstClass> MstClass = null!;
        private readonly IMongoCollection<MstSvtCard> MstSvtCard = null!;
        private readonly IMongoCollection<MstSkill> MstSkill = null!;
        private readonly IMongoCollection<MstSvtTreasureDevice> MstSvtTreasureDevice = null!;
        private readonly IMongoCollection<MstBuff> MstBuff = null!;
        private readonly IMongoCollection<MstFunc> MstFunc = null!;
        private readonly IMongoCollection<MstEvent> MstEvent = null!;
        private readonly IMongoCollection<MstTreasureDeviceLv> MstTreasureDeviceLv = null!;
        private readonly IMongoCollection<MstAttriRelation> MstAttriRelation = null!;
        private readonly IMongoCollection<MstCombineLimit> MstCombineLimit = null!;
        private readonly IMongoCollection<MstCombineSkill> MstCombineSkill = null!;
        private readonly IMongoCollection<MstItem> MstItem = null!;
        private readonly IMongoCollection<MstSkillLv> MstSkillLv = null!;
        private readonly IMongoCollection<MstTreasureDevice> MstTreasureDevice = null!;
        private readonly IMongoCollection<MstQuest> MstQuest = null!;
        private readonly IMongoCollection<MstSpot> MstSpot = null!;
        private readonly IMongoCollection<MstWar> MstWar = null!;
        private readonly IMongoCollection<MstQuestPhase> MstQuestPhase = null!;
    }
}