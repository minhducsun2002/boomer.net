using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disqord;
using Disqord.Bot;
using Pepper.Services.FGO;
using Pepper.Structures.External.FGO;
using Pepper.Structures.External.FGO.Renderer;
using Qmmands;

namespace Pepper.Commands.FGO
{
    public class ServantSkill : ServantCommand
    {
        private static readonly LocalEmbedField Blank = new();

        public ServantSkill(MasterDataService m, TraitService t, ItemNamingService i, ServantNamingService n) : base(m, t, i, n) {}

        [Command("skill", "sk", "skills", "servant-skill", "servant-skills")]
        [Description("Show skills of a servant")]
        public DiscordCommandResult Exec([Remainder] [Description("A servant name, ID, or in-game number.")] ServantIdentity servantIdentity)
        {
            IMasterDataProvider jp = MasterDataService.Connections[Region.JP],
                                        na = MasterDataService.Connections[Region.NA];

            var servant = jp.GetServant(servantIdentity.ServantId);
            servant.Name = ResolveServantName(servant);
            
            var records = jp.GetServantSkillAssociationByServantId(servant.ID)
                .OrderBy(skillMapping => skillMapping.Priority)
                .GroupBy(skillMapping => skillMapping.Num)
                .OrderBy(skillGrouping => skillGrouping.First().Num)
                .ToList();

            var outputFields = new List<LocalEmbedField>();
            HashSet<int> referencedSkillIds = new();

            foreach (var position in records)
            {
                if (outputFields.Count != 0) outputFields.Add(Blank.WithBlankName().WithBlankValue());
                var skills = position.Select(mapping =>
                    {
                        var skill = jp.GetSkillById(mapping.SkillId);
                        var name = na.GetSkillById(mapping.SkillId)?.MstSkill.Name;
                        skill!.MstSkill.Name = name ?? skill.MstSkill.Name;
                        return skill;
                    })
                    .Select(skill => (skill, new SkillRenderer(skill.MstSkill, jp, skill).Prepare(TraitService, true)))
                    .Select(pair =>
                    {
                        var (skill, (effects, referencedSkills)) = pair;
                        var levels = skill.MstSkillLv.Select(level => level.ChargeTurn).Distinct().OrderByDescending(c => c);

                        var reference = new StringBuilder();
                        foreach (var (referencedSkill, description) in referencedSkills)
                            if (!referencedSkillIds.Contains(referencedSkill.MstSkill.ID))
                            {
                                referencedSkillIds.Add(referencedSkill.MstSkill.ID);
                                reference.AppendLine(
                                    $"[Skill {referencedSkill.MstSkill.ID}]\n" 
                                    + string.Join("\n", description.Select(line => $"→ {line}"))
                                    + "\n"
                                );
                            }
                        
                        
                        return new LocalEmbedField
                        {
                            Name = $"{skill.MstSkill.Name} ({string.Join("-", levels)})",
                            Value = string.Join("\n\n", effects) + "\n\n" + reference
                        };
                    })
                    .ToList();
                
                outputFields.AddRange(skills);
            }

            return Reply(servant.BaseEmbed().WithFields(outputFields));
        }
    }
}