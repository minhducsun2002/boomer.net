using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Pepper.Services.FGO;

namespace Pepper.Commands.FGO
{
    public partial class FreeQuest
    {
        private class Svt
        {
            [JsonProperty("uniqueId")] public int UniqueId;
            [JsonProperty("name")] public string Name;
            [JsonProperty("roleType")] public int RoleType;
            [JsonProperty("npcId")] public int NpcId;
            [JsonProperty("index")] public int Index;
            [JsonProperty("id")] public int Id;
            [JsonProperty("userSvtId")] public int UserSvtId;
            [JsonProperty("isFollowerSvt")] public bool IsFollowerSvt;
            [JsonProperty("npcFollowerSvtId")] public int NpcFollowerSvtId;
        }

        private class UserSvt
        {
            [JsonProperty("recover")] public int Recover;
            [JsonProperty("chargeTurn")] public int ChargeTurn;
            [JsonProperty("skillId1")] public int SkillId1;
            [JsonProperty("skillId2")] public int SkillId2;
            [JsonProperty("skillId3")] public int SkillId3;
            [JsonProperty("treasureDeviceId")] public int TreasureDeviceId;
            [JsonProperty("treasureDeviceLv")] public int TreasureDeviceLv;
            [JsonProperty("criticalRate")] public int CriticalRate;
            [JsonProperty("aiId")] public int AiId;
            [JsonProperty("actPriority")] public int ActPriority;
            [JsonProperty("maxActNum")] public int MaxActNum;
            [JsonProperty("npcSvtType")] public int NpcSvtType;
            [JsonProperty("starRate")] public int StarRate;
            [JsonProperty("tdRate")] public int TdRate;
            [JsonProperty("deathRate")] public int DeathRate;
            [JsonProperty("individuality")] public int[] Individuality = Array.Empty<int>();
            [JsonProperty("tdAttackRate")] public int TdAttackRate;
            [JsonProperty("id")] public int Id;
            [JsonProperty("userId")] public int UserId;
            [JsonProperty("svtId")] public int SvtId;
            [JsonProperty("lv")] public int Lv;
            [JsonProperty("exp")] public int Exp;
            [JsonProperty("atk")] public int Atk;
            [JsonProperty("hp")] public int Hp;
            [JsonProperty("skillLv1")] public int SkillLv1;
            [JsonProperty("skillLv2")] public int SkillLv2;
            [JsonProperty("skillLv3")] public int SkillLv3;
        }

        private class EnemyDeck
        {
            [JsonProperty("svts")] public Svt[] Svts = Array.Empty<Svt>();
        }
        
        private class Quest
        {
            [JsonProperty("battleId")] public int BattleId;
            [JsonProperty("region")] public Region Region;
            [JsonProperty("questId")] public int QuestId;
            [JsonProperty("questPhase")] public int QuestPhase;
            [JsonProperty("questSelect")] public int QuestSelect;
            [JsonProperty("eventId")] public int EventId;
            [JsonProperty("battleType")] public int BattleType;
            [JsonProperty("enemyDeck")] public EnemyDeck[] EnemyDeck = Array.Empty<EnemyDeck>();
            [JsonProperty("userSvt")] public UserSvt[] UserSvt = Array.Empty<UserSvt>();
        }
    }
}