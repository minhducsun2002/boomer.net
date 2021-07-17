using System.Linq;
using MongoDB.Driver;
using Pepper.Structures.External.FGO.Entities;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO
{
    public partial class MasterDataMongoDBConnection
    {
        public Skill GetSkillById(int id, MstSkill? mstSkillHint = null)
        {
            var mstSkill = mstSkillHint ?? MstSkill.FindSync(Builders<MstSkill>.Filter.Eq("id", id)).First();
            var levels = MstSkillLv
                .FindSync(Builders<MstSkillLv>.Filter.Eq("skillId", id))
                .ToList()
                .OrderBy(level => level.Level)
                .ToArray();
            var functions = levels[0].FuncId.Select(func => ResolveFunc(func)!);

            return new Skill(mstSkill)
            {
                Invocations = functions.ToDictionary(
                    function => function,
                    function => levels.Select(level => level.FuncToSvals[function.ID])
                        .Select(funcLevelInvocation => Renderer.DataValParser.Parse(funcLevelInvocation, function.Type))
                        .ToArray()
                )
            };
        }
    }
}