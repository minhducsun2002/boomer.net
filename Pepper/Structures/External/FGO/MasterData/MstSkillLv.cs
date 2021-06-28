using System;
using System.Collections.Immutable;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;

namespace Pepper.Structures.External.FGO.MasterData
{
    public class MstSkillLv : MasterDataEntity
    {
        [BsonElement("funcId")] public int[] FuncId = Array.Empty<int>();
        [BsonElement("svals")] public string[] Svals = Array.Empty<string>();
        [BsonElement("skillId")] public int SkillId;
        [BsonElement("lv")] public int Level;
        [BsonElement("chargeTurn")] public int ChargeTurn;
        [BsonElement("skillDetailId")] public int SkillDetailId;
        [BsonElement("priority")] public int Priority;

        public MstSkillLv()
        {
            FuncSvalsMap = new Lazy<ImmutableDictionary<int, string>>(
                () => FuncId.Select((funcId, index) => (funcId, Svals[index]))
                    .ToImmutableDictionary(tuple => tuple.Item1, tuple => tuple.Item2)
            );
        }

        public ImmutableDictionary<int, string> FuncToSvals => FuncSvalsMap.Value;
        private readonly Lazy<ImmutableDictionary<int, string>> FuncSvalsMap;
    }
}