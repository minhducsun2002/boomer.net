using System.Collections.Concurrent;
using System.Collections.Generic;
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
        public IEnumerable<int> AttributeList => MasterDataService.Connections[Region.JP].GetAttributeLists();
        private ConcurrentDictionary<int, ServantNaming> ServantNamings => servantNamingService.Namings;

        protected BaseServant ResolveServant(ServantIdentity servantIdentity)
        {
            IMasterDataProvider jp = MasterDataService.Connections[Region.JP], na = MasterDataService.Connections[Region.NA];
            
            var servant = jp.GetServant(servantIdentity.ServantId);

            // overwriting servant name
            servant.Name = ResolveServantName(servant);
            
            // overwriting class
            servant.Class = na.ResolveClass(servant.Class.ID) ?? servant.Class;
            return servant;
        }
        
        protected string ResolveServantName(ServantIdentity servantIdentity, BaseServant? hint = null)
        {
            if (servantNamingService.Namings.ContainsKey(servantIdentity))
                return servantNamingService.Namings[servantIdentity].Name;
            
            IMasterDataProvider na = MasterDataService.Connections[Region.NA],
                jp = MasterDataService.Connections[Region.JP];
            return na.GetServantEntityById(servantIdentity)?.Name ??
                   (hint?.ServantEntity.Name ?? jp.GetServantEntityById(servantIdentity)!.Name);
        }

        protected string ResolveServantName(BaseServant servant) =>
            ResolveServantName(new ServantIdentity { ServantId = servant.ID }, servant);
    }
}