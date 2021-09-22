using MongoDB.Bson.Serialization.Attributes;

namespace Pepper.Structures.External.FGO.MasterData
{
    public class MstWar : MasterDataEntity
    {
        [BsonElement("id")] public int ID;
        [BsonElement("age")] public string Age = "";
        [BsonElement("name")] public string Name = "";
        [BsonElement("longName")] public string LongName = "";
    }
}