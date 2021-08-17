using System;
using FgoExportedConstants;
using MongoDB.Bson.Serialization.Attributes;

namespace Pepper.Structures.External.FGO.MasterData
{
    public class MstSvt : MasterDataEntity
    {
        [BsonElement("relateQuestIds")] public int[] RelateQuestIds = Array.Empty<int>();
        [BsonElement("individuality")] public int[] Traits = Array.Empty<int>();
        [BsonElement("classPassive")] public int[] ClassPassive = Array.Empty<int>();
        [BsonElement("cardIds")] public int[] CardIds = Array.Empty<int>();
        [BsonElement("id")] public int ID;
        [BsonElement("baseSvtId")] public int BaseSvtId;
        [BsonElement("name")] public string Name = "";
        [BsonElement("ruby")] public string Ruby = "";
        [BsonElement("battleName")] public string BattleName = "";
        [BsonElement("classId")] public int ClassId;
        [BsonElement("type")] public SvtType.Type Type;
        [BsonElement("limitMax")] public int LimitMax;
        [BsonElement("rewardLv")] public int RewardLv;
        [BsonElement("friendshipId")] public int FriendshipId;
        [BsonElement("maxFriendshipRank")] public int MaxFriendshipRank;
        [BsonElement("genderType")] public int GenderType;
        [BsonElement("attri")] public int Attri;
        [BsonElement("combineSkillId")] public int CombineSkillId;
        [BsonElement("combineLimitId")] public int CombineLimitId;
        [BsonElement("sellQp")] public int SellQp;
        [BsonElement("sellMana")] public int SellMana;
        [BsonElement("sellRarePri")] public int SellRarePri;
        [BsonElement("expType")] public int ExpType;
        [BsonElement("combineMaterialId")] public int CombineMaterialId;
        [BsonElement("cost")] public int Cost;
        [BsonElement("battleSize")] public int BattleSize;
        [BsonElement("hpGaugeY")] public int HpGaugeY;
        [BsonElement("starRate")] public int StarRate;
        [BsonElement("deathRate")] public int DeathRate;
        [BsonElement("attackAttri")] public int AttackAttri;
        [BsonElement("illustratorId")] public int IllustratorId;
        [BsonElement("cvId")] public int CvId;
        [BsonElement("collectionNo")] public int CollectionNo;
        [BsonElement("materialStoryPriority")] public int MaterialStoryPriority;
        [BsonElement("flag")] public int Flag;
    }
}