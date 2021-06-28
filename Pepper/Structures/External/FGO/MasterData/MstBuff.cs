using System;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
using Pepper.Services.FGO;

namespace Pepper.Structures.External.FGO.MasterData
{
    public class MstBuff : MasterDataEntity
    {
        [BsonElement("vals")] public int[] Vals = Array.Empty<int>();
        [BsonElement("tvals")] public int[] Tvals = Array.Empty<int>();
        [BsonElement("ckSelfIndv")] public int[] CkSelfIndv = Array.Empty<int>();
        [BsonElement("ckOpIndv")] public int[] CkOpIndv = Array.Empty<int>();
        [BsonElement("id")] public int ID;
        [BsonElement("buffGroup")] public int BuffGroup;
        [BsonElement("type")] public int Type;
        [BsonElement("name")] public string Name = "";
        [BsonElement("detail")] public string Detail = "";
        [BsonElement("iconId")] public int IconId;
        [BsonElement("maxRate")] public int MaxRate;
        [BsonElement("effectId")] public int EffectId;
    }

    public static class MstBuffExtensions
    {
        public static string[] GetSelfTraits(this MstBuff buff, TraitService traitService)
        {
            return buff.CkSelfIndv.Select(t => traitService.GetTrait(t)).ToArray();
        }
        
        public static string[] GetOpponentTraits(this MstBuff buff, TraitService traitService)
        {
            return buff.CkSelfIndv.Select(t => traitService.GetTrait(t)).ToArray();
        }
    }
}