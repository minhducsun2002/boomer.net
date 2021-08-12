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
        protected readonly TraitService TraitService;
        protected readonly ItemNamingService ItemNamingService;

        public FGOCommand(
            MasterDataService masterDataService,
            TraitService traitService,
            ItemNamingService itemNamingService)
        {
            MasterDataService = masterDataService;
            TraitService = traitService;
            ItemNamingService = itemNamingService;
        }
        
        public ConcurrentDictionary<long, string> ItemNamings => ItemNamingService.Namings;
    }
}