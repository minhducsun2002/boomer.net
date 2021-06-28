using MongoDB.Bson.Serialization.Attributes;

namespace Pepper.Structures.External.FGO.MasterData
{
    public class MstClass : MasterDataEntity
    {
        [BsonElement("id")] public int ID;
        [BsonElement("attri")] public int Attri;
        [BsonElement("name")] public string Name = "";
        [BsonElement("individuality")] public int Individuality;
        [BsonElement("attackRate")] public int AttackRate;
        [BsonElement("imageId")] public int ImageId;
        [BsonElement("iconImageId")] public int IconImageId;
        [BsonElement("frameId")] public int FrameId;
        [BsonElement("priority")] public int Priority;
        [BsonElement("groupType")] public int GroupType;
        [BsonElement("relationId")] public int RelationId;
        [BsonElement("supportGroup")] public int SupportGroup;
        [BsonElement("autoSelSupportType")] public int AutoSelSupportType;
    }
}