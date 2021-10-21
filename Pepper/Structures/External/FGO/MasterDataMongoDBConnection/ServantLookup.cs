using System;
using System.Linq;
using Disqord;
using FgoExportedConstants;
using MongoDB.Driver;
using Pepper.Structures.External.FGO.Entities;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO
{
    public partial class MasterDataMongoDBConnection
    {
        public BaseServant GetServant(MstSvt svt) => GetServant(svt.ID, svt);
        public BaseServant GetServant(int id, MstSvt? hint = null)
        {
            var svt = hint ?? MstSvt.FindSync(Builders<MstSvt>.Filter.Eq("baseSvtId", id)).First();
            var limits = MstSvtLimit.FindSync(Builders<MstSvtLimit>.Filter.Eq("svtId", id)).ToList().ToArray();
            var @class = MstClass.FindSync(Builders<MstClass>.Filter.Eq("id", svt.ClassId),
                new FindOptions<MstClass> { Limit = 1 }).First();
            var attributes = GetAttributeLists();
            var cards = MstSvtCard.FindSync(Builders<MstSvtCard>.Filter.Eq("svtId", id)).ToList().ToArray();
            return new BaseServant(svt, limits, @class, svt.Traits.First(attributes.Contains), cards);
        }

        private readonly FilterDefinition<MstSvt> servantTypeFilter = Builders<MstSvt>.Filter.Or(
            Builders<MstSvt>.Filter.Eq("type", SvtType.Type.NORMAL),
            Builders<MstSvt>.Filter.Eq("type", SvtType.Type.HEROINE),
            Builders<MstSvt>.Filter.Eq("type", SvtType.Type.ENEMY_COLLECTION_DETAIL)
        );
        public MstSvt? GetServantEntityByCollectionNo(int collectionNo)
            => MstSvt.FindSync(Builders<MstSvt>.Filter.And(
                Builders<MstSvt>.Filter.Eq("collectionNo", collectionNo),
                servantTypeFilter
            )).FirstOrDefault();

        public MstSvt? GetServantEntityById(int id) => MstSvt.FindSync(Builders<MstSvt>.Filter.Eq("baseSvtId", id)).FirstOrDefault();

        public ServantLimits GetServantLimits(int servantId)
        {
            var ascensionLimits = MstCombineLimit.FindSync(
                Builders<MstCombineLimit>.Filter.Eq("id", servantId),
                new FindOptions<MstCombineLimit> { Limit = 4, Sort = Builders<MstCombineLimit>.Sort.Ascending("qp") }
            ).ToList().ToArray();
            var skillLimits = MstCombineSkill.FindSync(
                Builders<MstCombineSkill>.Filter.Eq("id", servantId),
                new FindOptions<MstCombineSkill> { Sort = Builders<MstCombineSkill>.Sort.Ascending("qp") }
            ).ToList().ToArray();
            return new ServantLimits(ascensionLimits, skillLimits);
        }

        public MstSvt[] GetAllServantEntities() => MstSvt.FindSync(servantTypeFilter).ToList().ToArray();
    }
}