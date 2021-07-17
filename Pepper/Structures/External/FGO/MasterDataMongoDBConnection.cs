using MongoDB.Driver;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO
{
    public partial class MasterDataMongoDBConnection
    {
        // The field names must match the class names (i.e. a field named MstCollection1 must be of type IMongoCollection<MstCollection1>)
        public IMongoCollection<MstSvt> MstSvt = null!;
        public IMongoCollection<MstSvtLimit> MstSvtLimit = null!;
        public IMongoCollection<MstSvtSkill> MstSvtSkill = null!;
        public IMongoCollection<MstClass> MstClass = null!;
        public IMongoCollection<MstSvtCard> MstSvtCard = null!;
        public IMongoCollection<MstSkill> MstSkill = null!;
        public IMongoCollection<MstSvtTreasureDevice> MstSvtTreasureDevice = null!;
        public IMongoCollection<MstBuff> MstBuff = null!;
        public IMongoCollection<MstFunc> MstFunc = null!;
        public IMongoCollection<MstEvent> MstEvent = null!;
        public IMongoCollection<MstTreasureDeviceLv> MstTreasureDeviceLv = null!;
        public IMongoCollection<MstAttriRelation> MstAttriRelation = null!;
        public IMongoCollection<MstCombineLimit> MstCombineLimit = null!;
        public IMongoCollection<MstCombineSkill> MstCombineSkill = null!;
        public IMongoCollection<MstItem> MstItem = null!;
        public IMongoCollection<MstSkillLv> MstSkillLv = null!;
    }
}