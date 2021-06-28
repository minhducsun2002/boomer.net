using MongoDB.Bson.Serialization.Attributes;

namespace Pepper.Structures.External.FGO.MasterData
{
    public class MstSvtLimit : MasterDataEntity
    {
        [BsonElement("weaponColor")] public int WeaponColor;
        [BsonElement("svtId")] public int SvtId;
        [BsonElement("limitCount")] public int LimitCount;
        [BsonElement("rarity")] public int Rarity;
        [BsonElement("lvMax")] public int LvMax;
        [BsonElement("weaponGroup")] public int WeaponGroup;
        [BsonElement("weaponScale")] public int WeaponScale;
        [BsonElement("effectFolder")] public int EffectFolder;
        [BsonElement("hpBase")] public int HpBase;
        [BsonElement("hpMax")] public int HpMax;
        [BsonElement("atkBase")] public int AtkBase;
        [BsonElement("atkMax")] public int AtkMax;
        [BsonElement("criticalWeight")] public int CriticalWeight;
        [BsonElement("power")] public int Power;
        [BsonElement("defense")] public int Defense;
        [BsonElement("agility")] public int Agility;
        [BsonElement("magic")] public int Magic;
        [BsonElement("luck")] public int Luck;
        [BsonElement("treasureDevice")] public int TreasureDevice;
        [BsonElement("policy")] public int Policy;
        [BsonElement("personality")] public int Personality;
        [BsonElement("deity")] public int Deity;
        [BsonElement("stepProbability")] public int StepProbability;
        [BsonElement("strParam")] public string StrParam = "";
    }

}