using Pepper.Services.FGO;

namespace Pepper.Commands.FGO
{
    public abstract class FGODataCommand : FGOCommand
    {
        protected readonly MasterDataService MasterDataService;
        protected readonly TraitService TraitService;

        public FGODataCommand(MasterDataService masterDataService, TraitService traitService)
        {
            MasterDataService = masterDataService;
            TraitService = traitService;
        }
    }
}