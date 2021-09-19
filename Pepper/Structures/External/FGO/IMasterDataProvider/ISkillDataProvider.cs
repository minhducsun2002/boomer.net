using Pepper.Structures.External.FGO.Entities;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO
{
    public interface ISkillDataProvider
    {
        public Skill? GetSkillById(int id, MstSkill? mstSkillHint = null);
        public MstSkill[] GetSkillEntityByActIndividuality(int individuality);

        public MstSvtSkill[] GetServantSkillAssociationBySkillId(int skillId);
        public MstSvtSkill[] GetServantSkillAssociationByServantId(int svtId);
    }
}