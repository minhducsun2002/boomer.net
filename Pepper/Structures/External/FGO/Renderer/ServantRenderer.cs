using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Disqord;
using FgoExportedConstants;
using MongoDB.Driver;
using Pepper.Services.FGO;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO.Renderer
{
    public class ServantRenderer : EntityRenderer<MstSvt>
    {
        private readonly MasterDataService masterDataService;
        public ServantNamingService ServantNamingService { get; set; } = null!;
        public TraitService TraitService { get; set; } = null!;
        public ItemNamingService ItemNamingService { get; set; } = null!;

        private readonly MstSvt servantEntity;
        private string servantName = "";
        private readonly Lazy<List<MstSvtLimit>> lazyLimits;
        private readonly Lazy<MstClass> lazyClass;
        private readonly Lazy<List<MstSvtCard>> cards;
        private readonly Lazy<List<MstCombineLimit>> lazyCombineLimits;
        private readonly Lazy<List<MstCombineSkill>> lazyCombineSkills;
        
        private int[] AttributeList => masterDataService.Connections[Region.JP].GetAttributeLists();
        private int SvtId => servantEntity.BaseSvtId;
        private List<MstSvtLimit> Limits => lazyLimits.Value;
        private MstClass Class => lazyClass.Value;
        private List<MstCombineLimit> CombineLimits => lazyCombineLimits.Value;
        private List<MstCombineSkill> CombineSkills => lazyCombineSkills.Value;

        public ServantRenderer(MstSvt servant, MasterDataService service) : base(servant, service.Connections[Region.JP])
        {
            servantEntity = servant;
            masterDataService = service;
            
            MasterDataMongoDBConnection jp = service.Connections[Region.JP], na = service.Connections[Region.NA];
            
            lazyLimits = new Lazy<List<MstSvtLimit>>(
                () => jp.MstSvtLimit.FindSync(Builders<MstSvtLimit>.Filter.Eq("svtId", SvtId))
                .ToList()
            );
            lazyClass = new Lazy<MstClass>(
                () => na.MstClass.FindSync(Builders<MstClass>.Filter.Eq("id", servantEntity.ClassId),
                        new FindOptions<MstClass> {Limit = 1}).First()
            );
            cards = new Lazy<List<MstSvtCard>>(
                () => jp.MstSvtCard.FindSync(Builders<MstSvtCard>.Filter.Eq("svtId", SvtId)).ToList()    
            );
            lazyCombineLimits = new Lazy<List<MstCombineLimit>>(
                () => jp.MstCombineLimit.FindSync(Builders<MstCombineLimit>.Filter.Eq("id", SvtId)).ToList()
            );
            lazyCombineSkills = new Lazy<List<MstCombineSkill>>(
                () => jp.MstCombineSkill.FindSync(Builders<MstCombineSkill>.Filter.Eq("id", SvtId)).ToList()
            );
        }

        public LocalEmbed[] Prepare()
        {
            var npGain = GetNPGain();
            var traits = new HashSet<int>(servantEntity.Traits);
            var attribute = AttributeList.First(attrib => traits.Contains(attrib));
            var svtLimits = Limits;
            (int, string[])[] ascensions = GetCombineLimits(), skills = GetCombineSkills();
            
            return new[]
            {
                BaseEmbed()
                    .WithFooter("General info")
                    .WithFields(
                        new LocalEmbedField
                        {
                            Name = "HP/ATK",
                            Value =
                                $"Base : {svtLimits[0].HpBase}/{svtLimits[0].AtkBase}\nMaximum : {svtLimits[0].HpMax}/{svtLimits[0].AtkMax}",
                            IsInline = true
                        },
                        new LocalEmbedField
                        {
                            Name = "NP generation",
                            Value = $"Per hit : **{(float) npGain.TdPoint / 100:F2}**%"
                                    + $"\nWhen attacked : **{(float) npGain.TdPointDef / 100:F2}**%",
                            IsInline = true
                        },
                        new LocalEmbedField
                        {
                            Name = "Critical stars",
                            Value = $"Weight : **{svtLimits[0].CriticalWeight}**"
                                    + $"\nGeneration : **{(float) servantEntity.StarRate / 10:F1}**%",
                            IsInline = true
                        },
                        new LocalEmbedField
                        {
                            Name = "Gender / Attribute",
                            Value = $"{TraitService.GetTrait(servantEntity.GenderType + 0)} / {TraitService.GetTrait(attribute)}",
                            IsInline = true
                        },
                        new LocalEmbedField
                        {
                            Name = "Traits",
                            Value = string.Join(", ", GetTraits().Select(trait => TraitService.GetTrait(trait)))
                        },
                        new LocalEmbedField
                        {
                            Name = "Cards / Damage distribution by %",
                            Value = string.Join('\n',
                                "```",
                                "   Card   | Hit counts",
                                string.Join(
                                    '\n',
                                    GetCardStatistics(cards.Value).Select(
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
                    ),
                BaseEmbed()
                    .WithDescription(ascensions.Length == 0 ? "No materials needed." : "")
                    .WithFields(
                        ascensions.Select((limit, index) => new LocalEmbedField
                        {
                            Name = $"Stage {index + 1} - {limit.Item1.ToString("n0", CultureInfo.InvariantCulture)} QP",
                            Value = string.Join('\n', limit.Item2)
                        }))
                    .WithFooter("Ascension materials"),
                BaseEmbed()
                    .WithDescription(ascensions.Length == 0 ? "No materials needed." : "")
                    .WithFields(
                        skills.Select((limit, index) => new LocalEmbedField
                        {
                            Name = $"Level {index + 2} - {limit.Item1.ToString("n0", CultureInfo.InvariantCulture)} QP",
                            Value = string.Join('\n', limit.Item2)
                        }))
                    .WithFooter("Skill materials")
            };
        }

        private IEnumerable<int> GetTraits()
        {
            var traits = new HashSet<int>(servantEntity.Traits);
            traits.Remove(SvtId);
            traits.Remove(servantEntity.GenderType);
            traits.Remove(servantEntity.ClassId + 99);
            foreach (var attrib in AttributeList) traits.Remove(attrib);
            return traits;
        }
        
        private (int, string[])[] GetCombineSkills()
        {
            CombineSkills.Sort((a, b) => a.SkillLv - b.SkillLv);
            return CombineSkills.Select(stage =>
            {
                var items = stage.ItemIds.Select(id =>
                    {
                        if (ItemNamingService.Namings.TryGetValue(id, out var name))
                            return name;
                        return masterDataService.Connections[Region.NA].GetItemName(id) ?? 
                               masterDataService.Connections[Region.JP].GetItemName(id)!;
                    })
                    .Select((name, index) => $"- **{stage.ItemNums[index]}**x **{name}**")
                    .ToArray();
                return (stage.QP, items);
            }).ToArray();
        }
        
        private (int, string[])[] GetCombineLimits()
        {
            CombineLimits.Sort((a, b) => a.SvtLimit - b.SvtLimit);
            return CombineLimits.Select(stage =>
            {
                var items = stage.ItemIds.Select(id =>
                {
                    if (ItemNamingService.Namings.TryGetValue(id, out var name))
                        return name;
                    return masterDataService.Connections[Region.NA].GetItemName(id) ?? 
                           masterDataService.Connections[Region.JP].GetItemName(id)!;
                })
                .Select((name, index) => $"- **{stage.ItemNums[index]}**x **{name}**")
                .ToArray();
                return (stage.QP, items);
            }).ToArray();
        }
        
        private MstTreasureDeviceLv GetNPGain()
        {
            var jp = masterDataService.Connections[Region.JP];
            var mapping = jp.MstSvtTreasureDevice.FindSync(
                Builders<MstSvtTreasureDevice>.Filter.And(
                    Builders<MstSvtTreasureDevice>.Filter.Eq("svtId", SvtId),
                    Builders<MstSvtTreasureDevice>.Filter.Eq("num", 1)
                ),
                new FindOptions<MstSvtTreasureDevice> {Limit = 1}
            ).First()!;
            return jp.MstTreasureDeviceLv.FindSync(
                Builders<MstTreasureDeviceLv>.Filter.Eq("treaureDeviceId", mapping.TreasureDeviceId),
                new FindOptions<MstTreasureDeviceLv> {Limit = 1}
            ).First()!;
        }

        private LocalEmbed BaseEmbed()
        {
            if (string.IsNullOrEmpty(servantName))
            {
                servantName = servantEntity.Name;
                if (ServantNamingService.Namings.ContainsKey(SvtId))
                    servantName = ServantNamingService.Namings[SvtId].Name;
                else
                {
                    var record = masterDataService.Connections[Region.NA]
                        .MstSvt.FindSync(Builders<MstSvt>.Filter.Eq("baseSvtId", SvtId)).FirstOrDefault();
                    if (record != default) servantName = record.Name;
                }
            }
            
            return new LocalEmbed()
            {
                Author = new LocalEmbedAuthor
                {
                    Name = $"{Limits.Select(limit => limit.Rarity).Max()}☆ {Class.Name}",
                    IconUrl =
                        $"https://assets.atlasacademy.io/GameData/JP/ClassIcons/class3_{servantEntity.ClassId}.png"
                },
                Title = $"{servantEntity.CollectionNo}. **{servantName}** (`{SvtId}`)",
                Url = $"https://apps.atlasacademy.io/db/JP/servant/{servantEntity.CollectionNo}",
                ThumbnailUrl = $"https://assets.atlasacademy.io/GameData/JP/Faces/f_{SvtId}0.png"
            };
        }
        
        private static readonly Dictionary<BattleCommand.TYPE, Tuple<int, int[], string>> DefaultCardTypes = new()
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
    }
}