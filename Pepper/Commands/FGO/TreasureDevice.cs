using System.Collections.Generic;
using System.Linq;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus.Paged;
using FgoExportedConstants;
using MongoDB.Driver;
using Pepper.Services.FGO;
using Pepper.Structures;
using Pepper.Structures.External.FGO;
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
        
        public TreasureDevice(MasterDataService m, TraitService t, ItemNamingService i, ServantNamingService n) : base(m, t, i, n) {}

        [Command("np")]
        public DiscordCommandResult Exec(ServantIdentity servant)
        {
            MasterDataMongoDBConnection jp = MasterDataService.Connections[Region.JP],
                na = MasterDataService.Connections[Region.NA];

            var servantName = ResolveServantName(servant);
            
            var pages = jp.GetServantTreasureDevices(servant)
                .Where(map => map.Priority != 0)
                .OrderBy(map => map.TreasureDeviceId)
                .Select(map => (map, jp.GetTreasureDevice(map.TreasureDeviceId)))
                .Select(pair =>
                {
                    var (mapping, treasureDevice) = pair;
                    var mstTreasureDevice = treasureDevice.MstTreasureDevice;
                    var naRecord = na.MstTreasureDevice.Find(Builders<MstTreasureDevice>.Filter.Eq("id", mstTreasureDevice.ID)).FirstOrDefault();
                    
                    var name = naRecord?.Name ?? mstTreasureDevice.Name;
                    var typeText = naRecord?.TypeText ?? mstTreasureDevice.TypeText;
                    
                    Servant.DefaultCardTypes.TryGetValue((BattleCommand.TYPE) mapping.CardId, out var data);

                    var invocations = RenderInvocations(treasureDevice, jp);
                    var embed = new LocalEmbed
                    {
                        Author = new LocalEmbedAuthor().WithName(servantName),
                        Title = $"[{typeText}] {name} [__{mstTreasureDevice.Rank}__]",
                        Description = 
                            $"Card : **{data?.Item3.Trim() ?? "Unknown"}** - Hit count : **{mapping.Damage.Length}** ({string.Join('-', mapping.Damage)})",
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
            
            // TODO : Add buttons to quests
            return View(new SelectionPagedView(pages));
        }

        private List<InvocationInformation> RenderInvocations(Structures.External.FGO.Entities.TreasureDevice treasureDevice, MasterDataMongoDBConnection connection)
        {
            var functions = treasureDevice.functions;
            var _ = functions
                .Where(EnemyActionFilter.IsPlayerAction)
                .Select(function =>
                {
                    var mutationTypeHint = new Dictionary<string, TreasureDeviceMutationType>();
    
                    var data = treasureDevice.FuncToLevelsWithOvercharges[function];
                    var oc1 = data.Select(level => level.Item2[0]).ToArray();
                    var level1 = data[0].Item2;
    
                    foreach (var key in oc1[0].Keys)
                        if (oc1.Select(lvl => lvl[key]).Distinct().Count() > 1)
                            mutationTypeHint[key] = TreasureDeviceMutationType.Level;
    
                    foreach (var key in level1[0].Keys)
                        if (level1.Select(oc => oc[key]).Distinct().Count() > 1)
                            mutationTypeHint[key] = TreasureDeviceMutationType.Overcharge;
    
                    var baseInvocationArguments = oc1[0].ToDictionary(kv => kv.Key, kv => new[] { kv.Value });
    
                    foreach (var (key, mutationType) in mutationTypeHint)
                        baseInvocationArguments[key] = mutationType switch
                        {
                            TreasureDeviceMutationType.Level => oc1.Select(dataVal => dataVal[key]).ToArray(),
                            TreasureDeviceMutationType.Overcharge => level1.Select(dataVal => dataVal[key]).ToArray(),
                            _ => baseInvocationArguments[key]
                        };
    
                    return new InvocationRenderer(function, baseInvocationArguments, connection, TraitService).Render(mutationTypeHint);
                });

            return _.ToList();
        }
    }
}