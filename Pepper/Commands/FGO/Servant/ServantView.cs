using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Pepper.Services.FGO;
using Pepper.Structures.External.FGO;

using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Commands.FGO
{
    internal partial class ServantView : ViewBase
    {
        private readonly LocalEmbed general;
        private LocalEmbed Passive;
        private LocalEmbed ascItem;
        private LocalEmbed skillItem;
        
        public ServantView(
            (MstSvt, MstSvtLimit[], MstClass) servant,
            MstTreasureDeviceLv npGain,
            IReadOnlyList<MstSvtCard> cards,
            IReadOnlyList<MstCombineLimit> ascensionLimits, IReadOnlyList<MstCombineSkill> skillLimits,
            TraitService traitService,
            IReadOnlyList<int> attributes,
            IReadOnlyDictionary<int, string> itemNames,
            List<(Structures.External.FGO.Entities.Skill, List<string>)>? passives = null,
            (string, int, int, IEnumerable<string>)? bondCE = null,
            Snowflake? replyingTo = null
        ) : base(new LocalMessage())
        {
            var (svt, svtLimits, _) = servant;
            general = servant.BaseEmbed()
                .WithFooter("General info")
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
                        Value = $@"{traitService.GetTrait(svt.GenderType + 0)} / {traitService.GetTrait(svt.Traits.First(attributes.Contains))}",
                        IsInline = true
                    },
                    new LocalEmbedField
                    {
                        Name = "Traits",
                        Value = string.Join(", ", GetTraits(svt, attributes).Select(trait => traitService.GetTrait(trait)))
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
                .WithDescription(ascensionLimits.Count == 0 ? "No materials needed." : "")
                .WithFields(
                    ascensionLimits.Select((limit, index) => new LocalEmbedField
                    {
                        Name = $"Stage {index + 1} - {limit.QP.ToString("n0", CultureInfo.InvariantCulture)} QP",
                        Value = string.Join(
                            '\n',
                            limit.ItemIds.Zip(limit.ItemNums)
                                .Select(tuple => $"- **{tuple.Second}**x **{itemNames[tuple.First]}**")
                        )
                    }))
                .WithFooter("Ascension materials");
            
            skillItem = servant.BaseEmbed()
                .WithDescription(skillLimits.Count == 0 ? "No materials needed." : "")
                .WithFields(
                    skillLimits.Select((limit, index) => new LocalEmbedField
                    {
                        Name = $"Stage {index + 2} - {limit.QP.ToString("n0", CultureInfo.InvariantCulture)} QP",
                        Value = string.Join(
                            '\n',
                            limit.ItemIds.Zip(limit.ItemNums)
                                .Select(tuple => $"- **{tuple.Second}**x **{itemNames[tuple.First]}**")
                        )
                    }))
                .WithFooter("Skill materials");

            if (passives != null && passives.Count != 0)
                Passive = servant.BaseEmbed()
                    .WithFields(passives.Select(skill => new LocalEmbedField
                    {
                        Name = skill.Item1.MstSkill.Name,
                        Value = string.Join('\n', skill.Item2)
                    }));
            
            TemplateMessage = new LocalMessage().WithEmbeds(general);
            if (replyingTo != null) TemplateMessage = TemplateMessage.WithReply(replyingTo.Value);
            
            foreach (var (embed, label, index) in new[]
            {
                (general, "General info", 0),
                (Passive, "Passive skills", 1),
                (ascItem, "Ascension materials", 2),
                (skillItem, "Skill materials", 3)
            })
                if (embed != null)
                    AddComponent(new ButtonViewComponent(e =>
                    {
                        foreach (var component in EnumerateComponents())
                            if (component is ButtonViewComponent button)
                                button.IsDisabled = false;
                        e.Button.IsDisabled = true;
                        TemplateMessage.Embeds = new List<LocalEmbed> {embed};
                        return default;
                    })
                    {
                        Label = label,
                        Position = index,
                        // initial state
                        IsDisabled = index == 0
                    });
        }
    }
}