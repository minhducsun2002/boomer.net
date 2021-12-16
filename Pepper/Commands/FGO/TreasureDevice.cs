using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;
using FgoExportedConstants;
using MongoDB.Driver;
using Pepper.Services.FGO;
using Pepper.Structures;
using Pepper.Structures.External.FGO;
using Pepper.Structures.External.FGO.Entities;
using Pepper.Structures.External.FGO.MasterData;
using Pepper.Structures.External.FGO.Renderer;
using Qmmands;

namespace Pepper.Commands.FGO
{
    public class TreasureDevice : ServantCommand
    {
        private static readonly Dictionary<TreasureDeviceMutationType, string> HintPrefix = new()
        {
            { TreasureDeviceMutationType.Level, "<LVL>" },
            { TreasureDeviceMutationType.Overcharge, "<OC>" }
        };

        public TreasureDevice(MasterDataService m, TraitService t, ItemNamingService i, ServantNamingService n) : base(m, t, i, n) { }

        [Command("np", "snp", "servant-np")]
        [Description("View information about a servant's Noble Phantasms.")]
        public DiscordCommandResult Exec([Remainder][Description("A servant name, ID, or in-game number.")] ServantIdentity servant)
        {
            IMasterDataProvider jp = MasterDataService.Connections[Region.JP], na = MasterDataService.Connections[Region.NA];

            var servantName = ResolveServantName(servant);
            var pages = SerializePages(servant, servantName, jp, na);

            // TODO : Add buttons to quests
            return Menu(new DefaultMenu(new SelectionPagedView(pages)));
        }

        private IEnumerable<(LocalSelectionComponentOption selection, Page page)> SerializePages(
            int servantId,
            string servantName,
            IMasterDataProvider jp, IMasterDataProvider na
        ) => SerializePages(servantId, servantName, jp, na, TraitService);

        public static IEnumerable<(LocalSelectionComponentOption selection, Page page)> SerializePages<TProvider>(
            int servantId,
            string servantName,
            TProvider jp, TProvider na, ITraitNameProvider traitService
        ) where TProvider : ITreasureDeviceDataProvider, IQuestDataProvider, IItemDataProvider, IBaseObjectsDataProvider
        {
            var pages = jp.GetCachedServantTreasureDevices(servantId)
                .Where(map => map.Priority != 0)
                .OrderBy(map => map.TreasureDeviceId)
                .Select(map => (map, jp.GetTreasureDevice(map.TreasureDeviceId)))
                .Select(pair =>
                {
                    var (mapping, treasureDevice) = pair;
                    var mstTreasureDevice = treasureDevice.MstTreasureDevice;
                    var naRecord = na.GetTreasureDeviceEntity(mstTreasureDevice.ID);

                    var name = naRecord?.Name ?? mstTreasureDevice.Name;
                    var typeText = naRecord?.TypeText ?? mstTreasureDevice.TypeText;

                    Servant.DefaultCardTypes.TryGetValue((BattleCommand.TYPE) mapping.CardId, out var data);

                    var invocations = RenderInvocations(treasureDevice, jp, traitService);

                    var condition = new StringBuilder();
                    {
                        if (mapping.CondLv != default)
                        {
                            condition.AppendLine($"Requires servant level to reach level {mapping.CondLv}");
                        }

                        if (mapping.CondQuestId != default)
                        {
                            var quest = jp.ResolveQuest(mapping.CondQuestId);
                            var naQuest = na.ResolveQuest(mapping.CondQuestId);
                            var questType = TypeNames.QuestTypeNames[(QuestEntity.enType) quest.Type];
                            condition.AppendLine(
                                $"Requires completion of quest [[__{questType}__] {naQuest?.Name ?? quest.Name}](https://apps.atlasacademy.io/db/JP/quest/{quest.IconId}/{mapping.CondQuestPhase})"
                            );
                        }
                    }

                    var embed = new LocalEmbed
                    {
                        Author = new LocalEmbedAuthor().WithName(servantName),
                        Title = $"[{typeText}] {name} [__{mstTreasureDevice.Rank}__]",
                        Description =
                            $"Card : **{data?.Item3.Trim() ?? "Unknown"}** - Hit count : **{mapping.Damage.Length}** ({string.Join('-', mapping.Damage)})"
                            + "\n"
                            + condition,
                        Fields = invocations.Select(invc =>
                        {
                            var effectPrefix = invc.EffectMutationType.HasValue
                                ? HintPrefix[invc.EffectMutationType.Value] + " "
                                : "";
                            var detail = string.Join(
                                "\n",
                                invc.Statistics.Select(kv =>
                                {
                                    var (key, values) = kv;
                                    return (invc.TreasureDeviceMutationTypeHint.TryGetValue(key, out var hint)
                                               ? HintPrefix[hint] + " "
                                               : "")
                                           + $"[**{key}**] : {string.Join(" / ", values.Distinct())}";
                                }).Concat(invc.ExtraInformation)
                            );
                            return new LocalEmbedField
                            {
                                Name = effectPrefix + invc.Effect + $" to **{TargetTypeText.ResolveText(invc.RawFunction.TargetType)}**",
                                Value = string.IsNullOrWhiteSpace(detail) ? "\u200b" : detail
                            };
                        }).ToList()
                    };

                    var page = new Page().WithEmbeds(embed);
                    var rank = $" [{mstTreasureDevice.Rank}]";
                    var label = name + rank;
                    var selection = new LocalSelectionComponentOption
                    {
                        Label = label.Length > LocalSelectionComponentOption.MaxLabelLength
                            ? name[..(LocalSelectionComponentOption.MaxLabelLength - 3 - rank.Length)] + "..." + rank
                            : label
                    };

                    return (selection, page);
                });

            return pages;
        }

        private static List<InvocationInformation> RenderInvocations<TProvider>(
            Structures.External.FGO.Entities.TreasureDevice treasureDevice,
            TProvider connection, ITraitNameProvider traitService)
            where TProvider : IQuestDataProvider, IItemDataProvider, IBaseObjectsDataProvider
        {
            var functions = treasureDevice.Functions;
            var _ = functions
                .Select((func, index) => (func, index))
                .Where(pair => EnemyActionFilter.IsPlayerAction(pair.func) && pair.func.Type != (int) FuncList.TYPE.NONE)
                .Select(pair =>
                {
                    var (_, index) = pair;
                    var mutationTypeHint = new Dictionary<string, TreasureDeviceMutationType>();

                    var (function, datavals) = treasureDevice.FuncToLevelsWithOvercharges[index];
                    var oc1 = datavals.Select(level => level.Item2[0]).ToArray();
                    var level1 = datavals[0].Item2;

                    foreach (var key in oc1[0].Keys)
                    {
                        if (oc1.Select(lvl => lvl[key]).Distinct().Count() > 1)
                        {
                            mutationTypeHint[key] = TreasureDeviceMutationType.Level;
                        }
                    }

                    foreach (var key in level1[0].Keys)
                    {
                        if (level1.Select(oc => oc[key]).Distinct().Count() > 1)
                        {
                            mutationTypeHint[key] = TreasureDeviceMutationType.Overcharge;
                        }
                    }

                    var baseInvocationArguments = oc1[0].ToDictionary(kv => kv.Key, kv => new[] { kv.Value });

                    foreach (var (key, mutationType) in mutationTypeHint)
                    {
                        baseInvocationArguments[key] = mutationType switch
                        {
                            TreasureDeviceMutationType.Level => oc1.Select(dataVal => dataVal[key]).ToArray(),
                            TreasureDeviceMutationType.Overcharge => level1.Select(dataVal => dataVal[key]).ToArray(),
                            _ => baseInvocationArguments[key]
                        };
                    }

                    return new InvocationRenderer<TProvider>(function, baseInvocationArguments, connection, traitService).Render(mutationTypeHint);
                });

            return _.ToList();
        }
    }
}