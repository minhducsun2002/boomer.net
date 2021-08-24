using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus;
using FgoExportedConstants;
using Qmmands;
using MongoDB.Driver;
using Pepper.Services.FGO;
using Pepper.Structures;
using Pepper.Structures.Commands;
using Pepper.Structures.External.FGO;
using Pepper.Structures.External.FGO.Entities;
using Pepper.Structures.External.FGO.MasterData;
using Pepper.Structures.External.FGO.Renderer;

namespace Pepper.Commands.FGO
{
    public partial class Servant : ServantCommand
    {
        public Servant(MasterDataService m, TraitService t, ItemNamingService i, ServantNamingService n) : base(m, t, i, n) {}
        
        [Command("servant", "s")]
        [PrefixCategory("fgo")]
        public DiscordCommandResult Exec([Remainder] ServantIdentity servantIdentity)
        {
            MasterDataMongoDBConnection jp = MasterDataService.Connections[Region.JP], na = MasterDataService.Connections[Region.NA];
            
            var servant = jp.GetServant(servantIdentity.ServantId);

            // overwriting servant name
            servant.Name = ResolveServantName(servant);
            
            // overwriting class
            servant.Class = na.ResolveClass(servant.Class.ID) ?? servant.Class;
            
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
                        if (ItemNamingService.Namings.TryGetValue(itemId, out var name)) return name;
                        return na.GetItemName(itemId) ?? jp.GetItemName(itemId)!;
                    }
                );

            var pages = new[]
            {
                GeneralPage(
                    servant,
                    jp.MstSvtCard.FindSync(Builders<MstSvtCard>.Filter.Eq("svtId", servant.ID)).ToList()
                ),
                AscensionsPage(servant, limits, itemNames),
                SkillLimitsPage(servant, limits, itemNames),
                PassivesPage(servant)
            }
                .Where(entry => entry.HasValue)
                .Select(entry => entry!.Value)
                .ToArray();

            return View(new SelectionPagedView(pages, new LocalMessage().WithReply(Context.Message.Id)));
        }
    }
}