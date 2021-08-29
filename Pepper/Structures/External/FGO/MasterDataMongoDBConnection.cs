using MongoDB.Driver;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO
{
    public partial class MasterDataMongoDBConnection : IMasterDataProvider
    {
        // The field names must match the class names (i.e. a field named MstCollection1 must be of type IMongoCollection<MstCollection1>)
        private IMongoCollection<MstSvt> MstSvt = null!;
        private IMongoCollection<MstSvtLimit> MstSvtLimit = null!;
        public IMongoCollection<MstSvtSkill> MstSvtSkill = null!;
        private IMongoCollection<MstClass> MstClass = null!;
        private IMongoCollection<MstSvtCard> MstSvtCard = null!;
        public IMongoCollection<MstSkill> MstSkill = null!;
        public IMongoCollection<MstSvtTreasureDevice> MstSvtTreasureDevice = null!;
        private IMongoCollection<MstBuff> MstBuff = null!;
        private IMongoCollection<MstFunc> MstFunc = null!;
        private IMongoCollection<MstEvent> MstEvent = null!;
        public IMongoCollection<MstTreasureDeviceLv> MstTreasureDeviceLv = null!;
        private IMongoCollection<MstAttriRelation> MstAttriRelation = null!;
        private IMongoCollection<MstCombineLimit> MstCombineLimit = null!;
        private IMongoCollection<MstCombineSkill> MstCombineSkill = null!;
        public IMongoCollection<MstItem> MstItem = null!;
        private IMongoCollection<MstSkillLv> MstSkillLv = null!;
        public IMongoCollection<MstTreasureDevice> MstTreasureDevice = null!;
        public IMongoCollection<MstQuest> MstQuest = null!;
    }
}