using System;
using System.Collections.Generic;
using System.Linq;
using FgoExportedConstants;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Commands.FGO
{
    internal partial class ServantView
    {
        private static IEnumerable<int> GetTraits(MstSvt servantEntity, IEnumerable<int> attributeList)
        {
            var traits = new HashSet<int>(servantEntity.Traits);
            traits.Remove(servantEntity.ID);
            traits.Remove(servantEntity.GenderType);
            traits.Remove(servantEntity.ClassId + 99);
            foreach (var attrib in attributeList) traits.Remove(attrib);
            return traits;
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