using MongoDB.Bson.Serialization.Attributes;

namespace Pepper.Structures.External.FGO.MasterData
{
    public class MstSpot : MasterDataEntity
    {
        [BsonElement("id")] public int ID;
        [BsonElement("warId")] public int WarId;
        [BsonElement("name")] public string Name = "";
    }
}