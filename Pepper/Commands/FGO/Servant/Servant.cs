using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus;
using FgoExportedConstants;
using Qmmands;
using MongoDB.Driver;
using Pepper.Services.FGO;
using Pepper.Structures.Commands;
using Pepper.Structures.External.FGO;
using Pepper.Structures.External.FGO.MasterData;
using Pepper.Structures.External.FGO.Renderer;

namespace Pepper.Commands.FGO
{
    public class Servant : FGOCommand
    {
        public Servant(
            MasterDataService masterDataService, ServantNamingService servantNamingService, TraitService traitService,
            ItemNamingService itemNamingService
        ) : base(masterDataService, servantNamingService, traitService, itemNamingService) {}

        [Command("servant", "s")]
        [PrefixCategory("fgo")]
        public DiscordCommandResult Exec(int id = 2)
        {
            MasterDataMongoDBConnection jp = MasterDataService.Connections[Region.JP], na = MasterDataService.Connections[Region.NA];
            
            var servantTuple = jp.GetServant(
                jp.MstSvt.FindSync(
                    Builders<MstSvt>.Filter.Or(
                        Builders<MstSvt>.Filter.Eq("collectionNo", id),
                        Builders<MstSvt>.Filter.Eq("baseSvtId", id)
                    )
                ).First()
            );
            var (svt, limits, _) = servantTuple;
            
            if (ServantNamingService.Namings.ContainsKey(svt.ID))
                svt.Name = ServantNamingService.Namings[svt.ID].Name;
            else
                svt.Name = na
                    .MstSvt.FindSync(Builders<MstSvt>.Filter.Eq("baseSvtId", svt.ID))
                    .FirstOrDefault()?.Name ?? svt.Name;

            var ascensionLimits = jp.MstCombineLimit.FindSync(
                Builders<MstCombineLimit>.Filter.Eq("id", svt.ID),
                new FindOptions<MstCombineLimit> { Limit = 4, Sort = Builders<MstCombineLimit>.Sort.Ascending("qp") }
            ).ToList();
            var skillLimits = jp.MstCombineSkill.FindSync(
                Builders<MstCombineSkill>.Filter.Eq("id", svt.ID),
                new FindOptions<MstCombineSkill> { Sort = Builders<MstCombineSkill>.Sort.Ascending("qp") }
            ).ToList();
            var itemNames = ascensionLimits
                .SelectMany(record => record.ItemIds)
                .Concat(skillLimits.SelectMany(record => record.ItemIds))
                .Distinct()
                .ToDictionary(
                    itemId => itemId,
                    itemId =>
                    {
                        if (ItemNamingService.Namings.TryGetValue(id, out var name)) return name;
                        return na.GetItemName(itemId) ?? jp.GetItemName(itemId)!;
                    }
                );
            
            return View(
                new ServantView(
                    (svt, limits, na.ResolveClass(svt.ClassId)!),
                    GetNPGain(servantTuple.Item1.ID),
                    jp.MstSvtCard.FindSync(Builders<MstSvtCard>.Filter.Eq("svtId", svt.ID)).ToList(),
                    ascensionLimits,
                    skillLimits,
                    TraitService,
                    jp.GetAttributeLists(),
                    itemNames,
                    Context.Message.Id
                )
            );
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