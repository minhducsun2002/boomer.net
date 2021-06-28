using MongoDB.Bson.Serialization.Attributes;

namespace Pepper.Structures.External.FGO.MasterData
{
    public class MstAttriRelation : MasterDataEntity
    {
        [BsonElement("atkAttri")] public int AtkAttribute;
        [BsonElement("defAttri")] public int DefAttribute;
        [BsonElement("attackRate")] public int AttackRate;
    }
}