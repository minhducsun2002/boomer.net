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
using Pepper.Structures.External.FGO.Entities;
using Pepper.Structures.External.FGO.MasterData;
using Pepper.Structures.External.FGO.Renderer;

namespace Pepper.Commands.FGO
{
    public class Servant : ServantCommand
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
            
            // preparing the CE
            (string, int, int, IEnumerable<string>)? ceDetails = null;
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
                    ceDetails = (naName ?? ceSvt.Name, ceSvt.CollectionNo, ceSvt.ID, skill);
                }
            }
            
            // preparing passive skills
            var passives = servant.ServantEntity.ClassPassive.Select(skillId => jp.GetSkillById(skillId))
                .Select(skill =>
                {
                    // overwrite with NA name
                    try { skill.MstSkill.Name = na.GetSkillById(skill.MstSkill.ID).MstSkill.Name; } catch { /* ignore */ }
                    
                    var (baseSkill, referencedSkills) = new SkillRenderer(skill.MstSkill, jp, skill).Prepare(TraitService);
                    return (skill, baseSkill, referencedSkills);
                }).ToList()!;

            return View(
                new ServantView(
                    servant,
                    GetNPGain(servant.ID),
                    jp.MstSvtCard.FindSync(Builders<MstSvtCard>.Filter.Eq("svtId", servant.ID)).ToList(),
                    limits,
                    TraitService,
                    itemNames,
                    passives,
                    ceDetails,
                    Context.Message.Id
                )
            );
        }
        
        private string ResolveServantName(BaseServant servant)
        {
            var mstSvt = servant.ServantEntity;
            if (ServantNamingService.Namings.ContainsKey(mstSvt.ID))
                return ServantNamingService.Namings[mstSvt.ID].Name;

            var na = MasterDataService.Connections[Region.NA];
            return na.MstSvt.FindSync(Builders<MstSvt>.Filter.Eq("baseSvtId", mstSvt.ID))
                .FirstOrDefault()?.Name ?? mstSvt.Name;
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