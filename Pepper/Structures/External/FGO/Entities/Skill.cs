using System.Collections.Generic;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO.Entities
{
    public class Skill
    {
        internal Skill(MstSkill skill, MstSkillLv[] levels) { MstSkill = skill; MstSkillLv = levels; }
        public readonly MstSkill MstSkill;
        public readonly MstSkillLv[] MstSkillLv;
        public Dictionary<MstFunc, DataVal[]> Invocations = new();
    }
}