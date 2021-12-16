using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus;
using FgoExportedConstants;
using MongoDB.Driver;
using Pepper.Services.FGO;
using Pepper.Structures;
using Pepper.Structures.Commands;
using Pepper.Structures.External.FGO;
using Pepper.Structures.External.FGO.Entities;
using Pepper.Structures.External.FGO.MasterData;
using Pepper.Structures.External.FGO.Renderer;
using Qmmands;

namespace Pepper.Commands.FGO
{
    public partial class Servant : ServantCommand
    {
        public Servant(MasterDataService m, TraitService t, ItemNamingService i, ServantNamingService n) : base(m, t, i, n) { }

        [Command("servant", "s", "servant-info")]
        [Description("View information about a servant.")]
        [PrefixCategory("fgo")]
        public DiscordCommandResult Exec([Remainder][Description("A servant name, ID, or in-game number.")] ServantIdentity servantIdentity)
        {
            IMasterDataProvider jp = MasterDataService.Connections[Region.JP], na = MasterDataService.Connections[Region.NA];

            var servant = ResolveServant(servantIdentity);

            // overwriting item names
            var limits = jp.GetServantLimits(servant.ID);
            var itemNames = limits.AscensionCombine
                .SelectMany(record => record.ItemIds)
                .Concat(limits.SkillCombine.SelectMany(record => record.ItemIds))
                .Distinct()
                .ToDictionary(
                    itemId => itemId,
                    itemId =>
                    {
                        if (ItemNamingService.Namings.TryGetValue(itemId, out var name))
                        {
                            return name;
                        }

                        return na.GetItemName(itemId) ?? jp.GetItemName(itemId)!;
                    }
                );

            var pages = new[]
            {
                GeneralPage(servant, jp.GetNPGain(servant.ID)),
                AscensionsPage(servant, limits, itemNames),
                SkillLimitsPage(servant, limits, itemNames),
                PassivesPage(servant)
            }
                .Where(entry => entry.HasValue)
                .Select(entry => entry!.Value)
                .Select(entry =>
                {
                    entry.Item2.Content =
                        "Search may not bring up the expected result."
                        + "Please use ss command first to search, then call this command again with servant ID.";
                    return entry;
                })
                .ToArray();

            return Menu(new DefaultMenu(new SelectionPagedView(pages, new LocalMessage().WithReply(Context.Message.Id))));
        }
    }
}