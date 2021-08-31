using System.Collections.Generic;
using System.Linq;
using Disqord;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Humanizer;
using Pepper.Services.FGO;
using Pepper.Structures.External.FGO;
using Pepper.Structures.External.FGO.Entities;
using Pepper.Structures.External.FGO.Renderer;

namespace Pepper.Commands.FGO
{
    public partial class Servant
    {
        private class ReferencedSkillEquality : IEqualityComparer<(Skill, List<string>)>
        {
            public bool Equals((Skill, List<string>) x, (Skill, List<string>) y)
            {
                return x.Item1.MstSkill.ID == y.Item1.MstSkill.ID;
            }

            public int GetHashCode((Skill, List<string>) obj)
            {
                return obj.GetHashCode();
            }
        }

        private static readonly IEqualityComparer<(Skill, List<string>)> ReferencedSkillComparer = new ReferencedSkillEquality();
        
        private (LocalSelectionComponentOption, Page)? PassivesPage(BaseServant servant)
        {
            IMasterDataProvider jp = MasterDataService.Connections[Region.JP], na = MasterDataService.Connections[Region.NA];
            var passives = servant.ServantEntity.ClassPassive.Select(skillId => jp.GetSkillById(skillId))
                .Select(skill =>
                {
                    skill!.MstSkill.Name = na.GetSkillById(skill.MstSkill.ID)?.MstSkill.Name ?? skill.MstSkill.Name;
                    var (baseSkill, referencedSkills) =
                        new SkillRenderer(skill.MstSkill, jp, skill).Prepare(TraitService);
                    return (skill, baseSkill, referencedSkills);
                }).ToList();
            
            if (passives.Count != 0)
            {
                var passive = servant.BaseEmbed()
                    .WithFields(passives.Select(skill => new LocalEmbedField
                    {
                        Name = skill.Item1.MstSkill.Name,
                        Value = string.Join('\n', skill.Item2)
                    }));
                    
                var relatedSkills = passives
                    .SelectMany(related => related.Item3).Distinct(ReferencedSkillComparer)
                    .Select(skill => new LocalEmbedField
                    {
                        Name = $"[Skill {skill.Item1.MstSkill.ID}]",
                        Value = string.Join("\n", skill.Item2)
                    })
                    .ToList();

                if (relatedSkills.Count != 0)
                {
                    passive.AddBlankField();
                    foreach (var relatedSkill in relatedSkills) passive.Fields.Add(relatedSkill);
                }

                return (
                    new LocalSelectionComponentOption { Label = $"Passive {(passives.Count > 1 ? "skill".Pluralize() : "skill")}" },
                    new Page().WithEmbeds(passive)
                );
            }

            return null;
        }
    }
}