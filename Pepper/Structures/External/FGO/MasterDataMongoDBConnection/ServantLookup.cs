using System;
using System.Linq;
using Disqord;
using MongoDB.Driver;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO
{
    public static class ServantBasicDetailExtensions
    {
        public static LocalEmbed BaseEmbed(this (MstSvt, MstSvtLimit[], MstClass) tuple)
        {
            var (mstSvt, mstSvtLimits, @class) = tuple;
            return new LocalEmbed
            {
                Author = new LocalEmbedAuthor
                {
                    Name = $"{mstSvtLimits.Select(limit => limit.Rarity).Max()}☆ {@class.Name}",
                    IconUrl =
                        $"https://assets.atlasacademy.io/GameData/JP/ClassIcons/class3_{mstSvt.ClassId}.png"
                },
                Title = $"{mstSvt.CollectionNo}. **{mstSvt.Name}** (`{mstSvt.ID}`)",
                Url = $"https://apps.atlasacademy.io/db/JP/servant/{mstSvt.CollectionNo}",
                ThumbnailUrl = $"https://assets.atlasacademy.io/GameData/JP/Faces/f_{mstSvt.ID}0.png"
            };
        }
    }
    
    public partial class MasterDataMongoDBConnection
    {
        public (MstSvt, MstSvtLimit[], MstClass) GetServant(MstSvt svt) => GetServant(svt.ID, svt);
        public (MstSvt, MstSvtLimit[], MstClass) GetServant(int id, MstSvt? hint = null)
        {
            var svt = hint ?? MstSvt.FindSync(Builders<MstSvt>.Filter.Eq("baseSvtId", id)).First();
            var limits = MstSvtLimit.FindSync(Builders<MstSvtLimit>.Filter.Eq("svtId", id)).ToList().ToArray();
            var @class = MstClass.FindSync(Builders<MstClass>.Filter.Eq("id", svt.ClassId),
                new FindOptions<MstClass> {Limit = 1}).First();
            return (svt, limits, @class);
        }
    }
}