using System.Collections.Generic;
using System.Linq;
using FgoExportedConstants;
using Pepper.Structures.External.FGO.Entities;

namespace Pepper.Structures.External.FGO.Renderer
{
    public class SkillReferenceEnumerator
    {
        private readonly MasterDataMongoDBConnection connection;
        private readonly Skill skill;
        private HashSet<int> init;
        public SkillReferenceEnumerator(Skill skill, MasterDataMongoDBConnection connection, ref HashSet<int> init)
        {
            this.connection = connection;
            this.skill = skill;
            this.init = init;
            init.Add(skill.MstSkill.ID);
        }

        public HashSet<int> Enumerate()
        {
            foreach (var (func, value) in skill.Invocations)
            {
                switch ((FuncList.TYPE) func.Type)
                {
                    case FuncList.TYPE.ADD_STATE:
                    case FuncList.TYPE.ADD_STATE_SHORT:
                        break;
                    default:
                        continue;
                }
                
                var buff = connection.ResolveBuff(func.Vals[0])!;
                
                switch ((BuffList.TYPE) buff.Type)
                {
                    case BuffList.TYPE.DEAD_FUNCTION:
                    case BuffList.TYPE.DELAY_FUNCTION:
                    case BuffList.TYPE.ENTRY_FUNCTION:
                    case BuffList.TYPE.GUTS_FUNCTION:
                    case BuffList.TYPE.NPATTACK_PREV_BUFF:
                    case BuffList.TYPE.SELFTURNEND_FUNCTION:
                    case BuffList.TYPE.COMMANDATTACK_FUNCTION:
                        var skillIds = value.Select(values => int.Parse(values["Value"])).Distinct().ToList();
                        foreach (var skillId in skillIds)
                        {
                            if (init.Contains(skillId)) continue;
                            init.Add(skillId);
                            foreach (var referencedSkillId in new SkillReferenceEnumerator(connection.GetSkillById(skillId), connection, ref init).Enumerate())
                                init.Add(referencedSkillId);
                        }
                        break;
                    default:
                        continue;
                }
            }

            return init;
        }
    }
}