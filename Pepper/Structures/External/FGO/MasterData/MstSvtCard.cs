using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Pepper.Structures.External.FGO.MasterData
{
    public class MstSvtCard : MasterDataEntity
    {
        [BsonElement("normalDamage")] public int[] NormalDamage = Array.Empty<int>();
        [BsonElement("singleDamage")] public int[] SingleDamage = Array.Empty<int>();
        [BsonElement("trinityDamage")] public int[] TrinityDamage = Array.Empty<int>();
        [BsonElement("unisonDamage")] public int[] UnisonDamage = Array.Empty<int>();
        [BsonElement("grandDamage")] public int[] GrandDamage = Array.Empty<int>();
        [BsonElement("attackIndividuality")] public int[] AttackIndividuality = Array.Empty<int>();
        [BsonElement("svtId")] public int SvtId;
        [BsonElement("cardId")] public int CardId;
        [BsonElement("motion")] public int Motion;
        [BsonElement("attackType")] public int AttackType;
    }
}