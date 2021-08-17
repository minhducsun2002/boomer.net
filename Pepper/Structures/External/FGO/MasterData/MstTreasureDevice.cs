using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Pepper.Structures.External.FGO.MasterData
{
    public class MstTreasureDevice : MasterDataEntity
    {
        [BsonElement("individuality")] public int[] Individuality = Array.Empty<int>();
        [BsonElement("id")] public int ID;
        [BsonElement("seqId")] public int SeqId;
        [BsonElement("name")] public string Name = string.Empty;
        [BsonElement("ruby")] public string Ruby = string.Empty;
        [BsonElement("rank")] public string Rank = string.Empty;
        [BsonElement("maxLv")] public int MaxLv;
        [BsonElement("typeText")] public string TypeText = string.Empty;
        [BsonElement("attackAttri")] public int AttackAttri;
        [BsonElement("effectFlag")] public int EffectFlag;
    }
}