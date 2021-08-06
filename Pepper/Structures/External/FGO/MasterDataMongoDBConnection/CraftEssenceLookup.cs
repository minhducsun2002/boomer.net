using System.Linq;
using MongoDB.Driver;
using Pepper.Structures.External.FGO.Entities;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO
{
    public partial class MasterDataMongoDBConnection
    {
        private CraftEssence? BuildCraftEssence(MstSvt? mstSvt = null)
        {
            if (mstSvt == null) return null;
            var ce = new CraftEssence(mstSvt);
            var associatedSkill = MstSvtSkill.FindSync(Builders<MstSvtSkill>.Filter.Eq("svtId", mstSvt.ID)).ToList()!;
            var condLimitCounts = associatedSkill.Select(assoc => assoc.CondLimitCount).ToArray();
            int baseLimit = condLimitCounts.Min(), maxLimit = condLimitCounts.Max();
            var skills = associatedSkill.Select(association => new
            {
                Association = association,
                Skill = GetSkillById(association.SkillId)
            }).ToArray();
            ce.BaseSkills = skills.Where(pair => pair.Association.CondLimitCount == baseLimit)
                .Select(pair => pair.Skill).ToArray();
            ce.MLBSkills = skills.Where(pair => pair.Association.CondLimitCount == maxLimit && pair.Association.CondLimitCount != baseLimit)
                .Select(pair => pair.Skill).ToArray();
            return ce;
        }

        public CraftEssence? GetCraftEssenceById(int id, MstSvt? mstSvtHint = null)
            => BuildCraftEssence(mstSvtHint ?? MstSvt.FindSync(Builders<MstSvt>.Filter.Eq("baseSvtId", id)).FirstOrDefault());
        
        public CraftEssence? GetCraftEssenceByCollectionNo(int collectionNo, MstSvt? mstSvtHint = null)
            => BuildCraftEssence(mstSvtHint ?? MstSvt.FindSync(
                Builders<MstSvt>.Filter.And(
                    Builders<MstSvt>.Filter.Eq("collectionNo", collectionNo),
                    Builders<MstSvt>.Filter.Eq("type", 6))
                ).FirstOrDefault());
    }
}