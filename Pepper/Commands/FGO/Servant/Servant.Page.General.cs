using System;
using System.Collections.Generic;
using System.Linq;
using Disqord;
using Disqord.Extensions.Interactivity.Menus.Paged;
using FgoExportedConstants;
using MongoDB.Driver;
using Pepper.Services.FGO;
using Pepper.Structures.External.FGO;
using Pepper.Structures.External.FGO.Entities;
using Pepper.Structures.External.FGO.MasterData;
using Pepper.Structures.External.FGO.Renderer;

namespace Pepper.Commands.FGO
{
    public partial class Servant
    {
        private (LocalSelectionComponentOption, Page)? GeneralPage(
            BaseServant servant,
            IReadOnlyList<MstSvtCard> cards
        )
        {
            var svtLimits = servant.Limits;
            var svt = servant.ServantEntity;
            var npGain = GetNPGain(servant.ID);
            var general = servant.BaseEmbed()
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
                        Value = $@"{TraitService.GetTrait(svt.GenderType + 0)} / {TraitService.GetTrait(servant.Attribute)}",
                        IsInline = true
                    },
                    new LocalEmbedField
                    {
                        Name = "Traits",
                        Value = string.Join(", ", servant.Traits.Select(trait => TraitService.GetTrait(trait)))
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

            var bondCE = ResolveBondCE(servant);
            if (bondCE != default) general.Fields.Add(bondCE); 
            
            return (new LocalSelectionComponentOption { Label = "General info" }, new Page().WithEmbeds(general));
        }

        private LocalEmbedField? ResolveBondCE(BaseServant servant)
        {
            MasterDataMongoDBConnection jp = MasterDataService.Connections[Region.JP], na = MasterDataService.Connections[Region.NA];
            var bondCESkill = jp.MstSkill.FindSync(Builders<MstSkill>.Filter.Eq("actIndividuality", servant.ID)).FirstOrDefault();
            if (bondCESkill != default)
            {
                var skillId = bondCESkill.ID;
                var skillMapping = jp.MstSvtSkill.FindSync(Builders<MstSvtSkill>.Filter.Eq("skillId", skillId)).FirstOrDefault();
                if (skillMapping != default)
                {
                    var svtQuery = Builders<MstSvt>.Filter.Eq("id", skillMapping.SvtId);
                    var ceSvt = jp.MstSvt.FindSync(svtQuery).First();
                    var naName = na.MstSvt.FindSync(svtQuery).FirstOrDefault()?.Name;
                    var mstSkill = jp.MstSkill.FindSync(Builders<MstSkill>.Filter.Eq("id", skillId)).First();
                    var (skill, referencedSkills) = new SkillRenderer(mstSkill, jp).Prepare(TraitService);
                    return new LocalEmbedField
                    {
                        Name = "Bond CE",
                        Value =
                            $"[[**{ceSvt.CollectionNo}**. **{naName ?? ceSvt.Name}**]](https://apps.atlasacademy.io/db/JP/craft-essence/{ceSvt.ID})\n" +
                            string.Join("\n\n", skill)
                    };
                }
            }

            return null;
        }
        
        public static readonly Dictionary<BattleCommand.TYPE, Tuple<int, int[], string>> DefaultCardTypes = new()
        {
            { BattleCommand.TYPE.ARTS, new Tuple<int, int[], string>(0, Array.Empty<int>(), "Arts  ") },
            { BattleCommand.TYPE.BUSTER, new Tuple<int, int[], string>(0, Array.Empty<int>(), "Buster") },
            { BattleCommand.TYPE.QUICK, new Tuple<int, int[], string>(0, Array.Empty<int>(), "Quick ") },
            { BattleCommand.TYPE.ADDATTACK, new Tuple<int, int[], string>(0, Array.Empty<int>(), "Extra ") }
        };
        private static Dictionary<BattleCommand.TYPE, Tuple<int, int[], string>> GetCardStatistics(IReadOnlyCollection<MstSvtCard> cards)
        {
            var ret = new Dictionary<BattleCommand.TYPE, Tuple<int, int[], string>>(DefaultCardTypes);
            foreach (var (type, record) in ret)
            {
                var count = cards.Aggregate(0, (acc, card) => acc + (card.CardId == (int) type ? 1 : 0));
                var damage = cards.First(card => card.CardId == (int) type).NormalDamage;
                ret[type] = new Tuple<int, int[], string>(count, damage, record.Item3);
            }

            return ret;
        }
        
        private MstTreasureDeviceLv GetNPGain(int svtId)
        {
            var jp = MasterDataService.Connections[Region.JP];
            var mapping = jp.MstSvtTreasureDevice.FindSync(
                Builders<MstSvtTreasureDevice>.Filter.And(
                    Builders<MstSvtTreasureDevice>.Filter.Eq("svtId", svtId),
                    Builders<MstSvtTreasureDevice>.Filter.Eq("num", 1)
                ),
                new FindOptions<MstSvtTreasureDevice> {Limit = 1}
            ).First()!;
            return jp.MstTreasureDeviceLv.FindSync(
                Builders<MstTreasureDeviceLv>.Filter.Eq("treaureDeviceId", mapping.TreasureDeviceId),
                new FindOptions<MstTreasureDeviceLv> {Limit = 1}
            ).First()!;
        }
    }
}