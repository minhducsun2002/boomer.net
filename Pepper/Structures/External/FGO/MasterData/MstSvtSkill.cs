using MongoDB.Bson.Serialization.Attributes;

namespace Pepper.Structures.External.FGO.MasterData
{
    public class MstSvtSkill : MasterDataEntity
    {
        [BsonElement("strengthStatus")] public int StrengthStatus;
        [BsonElement("skillNum")] public int SkillNum;
        [BsonElement("svtId")] public int SvtId;
        [BsonElement("num")] public int Num;
        [BsonElement("priority")] public int Priority;
        [BsonElement("skillId")] public int SkillId;
        [BsonElement("condQuestId")] public int CondQuestId;
        [BsonElement("condQuestPhase")] public int CondQuestPhase;
        [BsonElement("condLv")] public int CondLv;
        [BsonElement("condLimitCount")] public int CondLimitCount;
        [BsonElement("eventId")] public int EventId;
        [BsonElement("flag")] public int Flag;
    }
}