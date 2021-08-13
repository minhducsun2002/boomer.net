using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Pepper.Services.FGO;
using Pepper.Structures.External.FGO.Entities;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Commands.FGO
{
    internal partial class ServantView : ViewBase
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
        private readonly LocalEmbed? passive;
        private readonly LocalEmbed general;
        private readonly LocalEmbed ascItem;
        private readonly LocalEmbed skillItem;
        private int currentIndex;
        
        public ServantView(
            BaseServant servant,
            MstTreasureDeviceLv npGain,
            IReadOnlyList<MstSvtCard> cards,
            ServantLimits limits,
            TraitService traitService,
            IReadOnlyDictionary<int, string> itemNames,
            IReadOnlyCollection<(Skill, List<string>, List<(Skill, List<string>)>)>? passives = null,
            (string, int, int, IEnumerable<string>)? bondCE = null,
            Snowflake? replyingTo = null
        ) : base(new LocalMessage())
        {
            var svtLimits = servant.Limits;
            var svt = servant.ServantEntity;
            general = servant.BaseEmbed()
                .WithFields(
                    new LocalEmbedField
                    {
                        Name = "HP/ATK",
                        Value = $"Base : {svtLimits[0].HpBase}/{svtLimits[0].AtkBase}\nMaximum : {svtLimits[0].HpMax}/{svtLimits[0].AtkMax}",
                        IsInline = true
                    },
                    new LocalEmbedField
                    {
                        Name = "NP generation",
                        Value = $"Per hit : **{(float) npGain.TdPoint / 100:F2}**%\nWhen attacked : **{(float) npGain.TdPointDef / 100:F2}**%",
                        IsInline = true
                    },
                    new LocalEmbedField
                    {
                        Name = "Critical stars",
                        Value = $"Weight : **{svtLimits[0].CriticalWeight}**\nGeneration : **{(float) svt.StarRate / 10:F1}**%",
                        IsInline = true
                    },
                    new LocalEmbedField
                    {
                        Name = "Gender / Attribute",
                        Value = $@"{traitService.GetTrait(svt.GenderType + 0)} / {traitService.GetTrait(servant.Attribute)}",
                        IsInline = true
                    },
                    new LocalEmbedField
                    {
                        Name = "Traits",
                        Value = string.Join(", ", servant.Traits.Select(trait => traitService.GetTrait(trait)))
                    },
                    new LocalEmbedField
                    {
                        Name = "Cards / Damage distribution by %",
                        Value = string.Join('\n',
                            "```",
                            "   Card   | Hit counts",
                            string.Join(
                                '\n',
                                GetCardStatistics(cards).Select(
                                    card =>
                                    {
                                        var (count, damage, name) = card.Value;
                                        return $"{count}x {name} | {damage.Length} ({string.Join('-', damage)})";
                                    }
                                )
                            ),
                            "```"
                        )
                    }
                );

            if (bondCE != null)
            {
                var (name, collectionNo, id, skill) = bondCE.Value;
                general.Fields.Add(new LocalEmbedField
                {
                    Name = "Bond CE",
                    Value = $"[[**{collectionNo}**. **{name}**]](https://apps.atlasacademy.io/db/JP/craft-essence/{id})\n" + string.Join("\n\n", skill)
                });
            }

            ascItem = servant.BaseEmbed()
                .WithDescription(limits.AscensionCombine.Length == 0 ? "No materials needed." : "")
                .WithFields(
                    limits.AscensionCombine.Select((limit, index) => new LocalEmbedField
                    {
                        Name = $"Stage {index + 1} - {limit.QP.ToString("n0", CultureInfo.InvariantCulture)} QP",
                        Value = string.Join(
                            '\n',
                            limit.ItemIds.Zip(limit.ItemNums)
                                .Select(tuple => $"- **{tuple.Second}**x **{itemNames[tuple.First]}**")
                        )
                    }));

            skillItem = servant.BaseEmbed()
                .WithDescription(limits.SkillCombine.Length == 0 ? "No materials needed." : "")
                .WithFields(
                    limits.SkillCombine.Select((limit, index) => new LocalEmbedField
                    {
                        Name = $"Stage {index + 2} - {limit.QP.ToString("n0", CultureInfo.InvariantCulture)} QP",
                        Value = string.Join(
                            '\n',
                            limit.ItemIds.Zip(limit.ItemNums)
                                .Select(tuple => $"- **{tuple.Second}**x **{itemNames[tuple.First]}**")
                        )
                    }));

            if (passives != null && passives.Count != 0)
            {
                passive = servant.BaseEmbed()
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
            }
            
            TemplateMessage = new LocalMessage().WithEmbeds(general);
            if (replyingTo != null) TemplateMessage = TemplateMessage.WithReply(replyingTo.Value);
            
            var pages = new[]
            {
                (general, "General info", 0),
                (passive, "Passive skills", 1),
                (ascItem, "Ascension materials", 2),
                (skillItem, "Skill materials", 3)
            };
            
            AddComponent(new SelectionViewComponent(e =>
            {
                TemplateMessage = TemplateMessage.WithEmbeds(pages[currentIndex = int.Parse(e.SelectedOptions[0].Value)].Item1);
                e.Selection.Options = GetCurrentOptionList(pages);
                return default;
            })
            {
                MaximumSelectedOptions = 1,
                MinimumSelectedOptions = 1,
                Options = GetCurrentOptionList(pages)
            });
        }

        private List<LocalSelectionComponentOption> GetCurrentOptionList(IEnumerable<(LocalEmbed?, string, int)> pages)
        {
            var selections = new List<LocalSelectionComponentOption>();
            
            foreach (var (embed, label, index) in pages)
                if (embed != null)
                    selections.Add(new LocalSelectionComponentOption
                    {
                        Label = label,
                        Value = $"{index}",
                        IsDefault = currentIndex == index
                    });
            return selections;
        }
    }
}