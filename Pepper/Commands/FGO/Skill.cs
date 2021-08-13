using System.Collections.Generic;
using System.Linq;
using Disqord;
using Disqord.Bot;
using MongoDB.Driver;
using Pepper.Services.FGO;
using Pepper.Structures.External.FGO;
using Pepper.Structures.External.FGO.MasterData;
using Pepper.Structures.External.FGO.Renderer;
using Qmmands;

namespace Pepper.Commands.FGO
{
    public class ServantSkill : ServantCommand
    {
        private static readonly LocalEmbedField Blank = new();

        public ServantSkill(MasterDataService m, TraitService t, ItemNamingService i, ServantNamingService n) : base(m, t, i, n) {}

        [Command("skill")]
        [Description("Show skills of a servant")]
        public DiscordCommandResult Exec([Remainder] ServantIdentity servantIdentity)
        {
            MasterDataMongoDBConnection jp = MasterDataService.Connections[Region.JP],
                                        na = MasterDataService.Connections[Region.NA];

            var servant = jp.GetServant(servantIdentity.ServantId);
            
            var records = jp.MstSvtSkill.FindSync(Builders<MstSvtSkill>.Filter.Eq("svtId", servant.ID)).ToList()
                .OrderBy(skillMapping => skillMapping.Priority)
                .GroupBy(skillMapping => skillMapping.Num)
                .OrderBy(skillGrouping => skillGrouping.First().Num)
                .ToList();

            var outputFields = new List<LocalEmbedField>();
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
                        return new LocalEmbedField
                        {
                            Name = $"{skill.MstSkill.Name} ({string.Join("-", levels)})",
                            Value = string.Join("\n\n", effects)
                        };
                    })
                    .ToList();
                outputFields.AddRange(skills);
            }

            return Reply(servant.BaseEmbed().WithFields(outputFields));
        }
    }
}