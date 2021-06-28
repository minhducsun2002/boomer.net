using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Pepper.Structures.External.FGO.MasterData
{
    public class MstSkill : MasterDataEntity
    {
        [BsonElement("effectList")] public int[] EffectList = Array.Empty<int>();
        /// <summary>
        /// Individualities that the skill actor must possess for this skill to fire
        /// </summary>
        [BsonElement("actIndividuality")] public int[] ActIndividuality = Array.Empty<int>();
        [BsonElement("id")] public int ID;
        [BsonElement("type")] public int Type;
        [BsonElement("name")] public string Name = "";
        [BsonElement("ruby")] public string Ruby = "";
        [BsonElement("maxLv")] public int MaxLv;
        [BsonElement("iconId")] public int IconId;
        [BsonElement("motion")] public int Motion;
    }
}