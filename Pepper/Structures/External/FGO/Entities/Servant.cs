using System.Collections.Generic;
using System.Linq;
using Disqord;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO.Entities
{
    public class BaseServant
    {
        public readonly MstSvtLimit[] Limits;
        public readonly MstSvt ServantEntity;
        public readonly MstSvtCard[] Cards;
        public MstClass Class { get; set; }
        public readonly int Attribute;
        public readonly int[] Traits;
        public int ID => ServantEntity.ID;
        public string Name { get; set; }

        internal BaseServant(MstSvt mstSvt, MstSvtLimit[] limits, MstClass @class, int attribute, MstSvtCard[] cards)
        {
            Limits = limits;
            ServantEntity = mstSvt;
            Class = @class;
            Attribute = attribute;
            Traits = GetTrait(mstSvt, attribute);
            Name = mstSvt.Name;
            Cards = cards;
        }

        private static int[] GetTrait(MstSvt servantEntity, int attribute)
        {
            var traits = new HashSet<int>(servantEntity.Traits);
            traits.Remove(servantEntity.ID);
            traits.Remove(servantEntity.GenderType);
            traits.Remove(servantEntity.ClassId + 99);
            traits.Remove(attribute);
            return traits.ToArray();
        }

        public LocalEmbed BaseEmbed()
        {
            return new LocalEmbed
            {
                Author = new LocalEmbedAuthor
                {
                    Name = $"{Limits.Select(limit => limit.Rarity).Max()}☆ {Class.Name}",
                    IconUrl =
                        $"https://assets.atlasacademy.io/GameData/JP/ClassIcons/class3_{ServantEntity.ClassId}.png"
                },
                Title = $"{ServantEntity.CollectionNo}. **{Name}** (`{ID}`)",
                Url = $"https://apps.atlasacademy.io/db/JP/servant/{ServantEntity.CollectionNo}",
                ThumbnailUrl = $"https://assets.atlasacademy.io/GameData/JP/Faces/f_{ServantEntity.ID}0.png"
            };
        }
    }
}