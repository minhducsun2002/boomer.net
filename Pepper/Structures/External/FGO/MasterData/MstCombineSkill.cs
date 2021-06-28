using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Pepper.Structures.External.FGO.MasterData
{
    public class MstCombineSkill : MasterDataEntity
    {
        [BsonElement("itemIds")] public int[] ItemIds = Array.Empty<int>();
        [BsonElement("itemNums")] public int[] ItemNums = Array.Empty<int>();
        [BsonElement("id")] public int SkillID;
        [BsonElement("skillLv")] public int SkillLv;
        [BsonElement("qp")] public int QP;
    }
}