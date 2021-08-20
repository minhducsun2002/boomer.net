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
using Qmmands;

namespace Pepper.Commands.FGO
{
    public class TreasureDevice : ServantCommand
    {
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

                    var embed = new LocalEmbed
                    {
                        Author = new LocalEmbedAuthor().WithName(servantName),
                        Title = $"[{typeText}] {name} [__{mstTreasureDevice.Rank}__]",
                        Description = 
                            $"Card : **{data?.Item3.Trim() ?? "Unknown"}** - Hit count : **{mapping.Damage.Length}** ({string.Join('-', mapping.Damage)})"
                    };

                    var page = new Page().WithEmbeds(embed);
                    var rank = $" [{mstTreasureDevice.Rank}]";
                    var label = name + rank;
                    var selection = new LocalSelectionComponentOption
                    {
                        Label = label.Length > LocalSelectionComponentOption.MaxLabelLength
                            ? name[..(LocalSelectionComponentOption.MaxLabelLength - 3 - rank.Length)] + "..." + rank
                            : name
                    };

                    return (selection, page);
                });
            
            // TODO : Add buttons to quests
            return View(new SelectionPagedView(pages));
        }
    }
}