using System;
using MongoDB.Bson.Serialization.Attributes;
namespace Pepper.Structures.External.FGO.MasterData
{
    public class MstSvtTreasureDevice : MasterDataEntity
    {
        [BsonElement("damage")] public int[] Damage = Array.Empty<int>();
        [BsonElement("strengthStatus")] public int StrengthStatus;
        [BsonElement("svtId")] public int SvtId;
        [BsonElement("num")] public int Num;
        [BsonElement("priority")] public int Priority;
        [BsonElement("flag")] public int Flag;
        [BsonElement("imageIndex")] public int ImageIndex;
        [BsonElement("treasureDeviceId")] public int TreasureDeviceId;
        [BsonElement("condQuestId")] public int CondQuestId;
        [BsonElement("condQuestPhase")] public int CondQuestPhase;
        [BsonElement("condLv")] public int CondLv;
        [BsonElement("condFriendshipRank")] public int CondFriendshipRank;
        [BsonElement("motion")] public int Motion;
        [BsonElement("cardId")] public int CardId;
    }
}