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

        public FGOCommand(MasterDataService masterDataService, TraitService traitService)
        {
            MasterDataService = masterDataService;
            TraitService = traitService;
        }
    }
}