using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Pepper.Structures.External.FGO.MasterData
{
    public class MstFunc : MasterDataEntity
    {
        [BsonElement("vals")] public int[] Vals = Array.Empty<int>();
        [BsonElement("tvals")] public int[] Tvals = Array.Empty<int>();
        [BsonElement("questTvals")] public int[] QuestTvals = Array.Empty<int>();
        [BsonElement("effectList")] public int[] EffectList = Array.Empty<int>();
        [BsonElement("popupTextColor")] public int PopupTextColor;
        [BsonElement("id")] public int ID;
        [BsonElement("cond")] public int Cond;
        [BsonElement("funcType")] public int Type;
        [BsonElement("targetType")] public int TargetType;
        [BsonElement("applyTarget")] public int ApplyTarget;
        [BsonElement("popupIconId")] public int PopupIconId;
        [BsonElement("popupText")] public string PopupText = "";
        [BsonElement("categoryId")] public int CategoryId;
    }
}