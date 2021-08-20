using System.Collections.Concurrent;
using MongoDB.Driver;
using Pepper.Services.FGO;
using Pepper.Structures.External.FGO;
using Pepper.Structures.External.FGO.Entities;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Commands.FGO
{
    public class ServantCommand : FGOCommand
    {
        public ServantCommand(
            MasterDataService m, TraitService t, ItemNamingService i,
            ServantNamingService naming
            ) : base(m, t, i)
        {
            servantNamingService = naming;
        }
        
        private readonly ServantNamingService servantNamingService;
        public int[] AttributeList => MasterDataService.Connections[Region.JP].GetAttributeLists();
        private ConcurrentDictionary<int, ServantNaming> ServantNamings => servantNamingService.Namings;

        protected string ResolveServantName(ServantIdentity servantIdentity, BaseServant? hint = null)
        {
            if (servantNamingService.Namings.ContainsKey(servantIdentity))
                return servantNamingService.Namings[servantIdentity].Name;
            
            MasterDataMongoDBConnection na = MasterDataService.Connections[Region.NA],
                jp = MasterDataService.Connections[Region.JP];
            return na.MstSvt.FindSync(Builders<MstSvt>.Filter.Eq("baseSvtId", servantIdentity))
                       .FirstOrDefault()?.Name ??
                   (hint?.ServantEntity.Name ?? jp.MstSvt.FindSync(Builders<MstSvt>.Filter.Eq("baseSvtId", servantIdentity)).First().Name);
        }

        protected string ResolveServantName(BaseServant servant) =>
            ResolveServantName(new ServantIdentity { ServantId = servant.ID }, servant);
    }
}