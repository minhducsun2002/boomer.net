using System.Collections.Concurrent;
using Pepper.Services.FGO;
using Pepper.Structures;
using Pepper.Structures.Commands;

namespace Pepper.Commands.FGO
{
    [Category("FGO")]
    [PrefixCategory("fgo")]
    public abstract class FGOCommand : Command
    {
        protected readonly MasterDataService MasterDataService;
        protected readonly ServantNamingService ServantNamingService;
        protected TraitService TraitService;
        protected ItemNamingService ItemNamingService;

        public FGOCommand(
            MasterDataService masterDataService, ServantNamingService servantNamingService, TraitService traitService,
            ItemNamingService itemNamingService)
        {
            MasterDataService = masterDataService;
            ServantNamingService = servantNamingService;
            TraitService = traitService;
            ItemNamingService = itemNamingService;
        }
        
        public int[] AttributeList => MasterDataService.Connections[Region.JP].GetAttributeLists();
        public ConcurrentDictionary<long, ServantNaming> ServantNamings => ServantNamingService.Namings;
        public ConcurrentDictionary<long, string> ItemNamings => ItemNamingService.Namings;
    }
}