using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Pepper.Structures.External.FGO.MasterData
{
    public class MstItem : MasterDataEntity
    {
        [BsonElement("individuality")] public int[] Individuality = Array.Empty<int>();
        [BsonElement("eventId")] public int EventId;
        [BsonElement("eventGroupId")] public int EventGroupId;
        [BsonElement("id")] public int ID;
        [BsonElement("name")] public string Name = "";
        [BsonElement("detail")] public string Detail = "";
        [BsonElement("imageId")] public int ImageId;
        [BsonElement("bgImageId")] public int BgImageId;
        [BsonElement("type")] public int Type;
        [BsonElement("unit")] public string Unit = "";
        [BsonElement("value")] public int Value;
        [BsonElement("sellQp")] public int SellQp;
        [BsonElement("isSell")] public bool IsSell;
        [BsonElement("priority")] public int Priority;
        [BsonElement("dropPriority")] public int DropPriority;
        [BsonElement("startedAt")] public int StartedAt;
        [BsonElement("endedAt")] public int EndedAt;
    }
}