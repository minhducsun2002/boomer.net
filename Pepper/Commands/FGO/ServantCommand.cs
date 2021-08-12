using System.Collections.Concurrent;
using Pepper.Services.FGO;

namespace Pepper.Commands.FGO
{
    public class ServantCommand : FGOCommand
    {
        protected readonly ServantNamingService ServantNamingService;
        public ServantCommand(
            MasterDataService m, TraitService t, ItemNamingService i,
            ServantNamingService naming
            ) : base(m, t, i)
        {
            ServantNamingService = naming;
        }
        
        public int[] AttributeList => MasterDataService.Connections[Region.JP].GetAttributeLists();
        public ConcurrentDictionary<int, ServantNaming> ServantNamings => ServantNamingService.Namings;
    }
}