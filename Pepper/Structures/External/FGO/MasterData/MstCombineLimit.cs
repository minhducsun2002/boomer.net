using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Pepper.Structures.External.FGO.MasterData
{
    public class MstCombineLimit : MasterDataEntity
    {
        [BsonElement("itemIds")] public int[] ItemIds = Array.Empty<int>();
        [BsonElement("itemNums")] public int[] ItemNums = Array.Empty<int>();
        [BsonElement("id")] public int ID;
        [BsonElement("svtLimit")] public int SvtLimit;
        [BsonElement("qp")] public int QP;
    }
}