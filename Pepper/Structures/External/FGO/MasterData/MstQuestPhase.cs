using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Pepper.Structures.External.FGO.MasterData
{
    public class MstQuestPhase : MasterDataEntity
    {
        [BsonElement("classIds")] public int[] ClassIds = Array.Empty<int>();
        [BsonElement("individuality")] public int[] Traits = Array.Empty<int>();
        [BsonElement("questId")] public int QuestId;
        [BsonElement("qp")] public int Qp;
        [BsonElement("playerExp")] public int PlayerEXP;
        [BsonElement("friendshipExp")] public int FriendshipEXP;
        [BsonElement("phase")] public int Phase; 
    }
}