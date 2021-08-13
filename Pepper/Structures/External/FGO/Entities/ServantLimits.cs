using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO.Entities
{
    public class ServantLimits
    {
        public readonly MstCombineLimit[] AscensionCombine;
        public readonly MstCombineSkill[] SkillCombine;

        internal ServantLimits(MstCombineLimit[] limits, MstCombineSkill[] skills)
        {
            AscensionCombine = limits;
            SkillCombine = skills;
        }
    }
}