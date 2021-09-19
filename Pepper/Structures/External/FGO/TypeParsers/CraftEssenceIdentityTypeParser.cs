using System.Linq;
using System.Threading.Tasks;
using Disqord.Bot;
using Pepper.Services.FGO;
using Pepper.Structures.External.FGO.Entities;
using Qmmands;

namespace Pepper.Structures.External.FGO.TypeParsers
{
    public class CraftEssenceIdentityTypeParser : DiscordTypeParser<CraftEssenceIdentity>
    {
        private readonly CraftEssenceNamingService craftEssenceNamingService;
        private readonly MasterDataService masterDataService;

        public CraftEssenceIdentityTypeParser(MasterDataService masterDataService, CraftEssenceNamingService craftEssenceNamingService)
        {
            this.masterDataService = masterDataService;
            this.craftEssenceNamingService = craftEssenceNamingService;
        }

        public override ValueTask<TypeParserResult<CraftEssenceIdentity>> ParseAsync(Parameter parameter, string value, DiscordCommandContext context)
        {
            if (int.TryParse(value, out var numericIdentity))
            {
                var result = ResolveNumericIdentifier(numericIdentity, masterDataService);
                return result == null 
                    ? Failure($"Could not find a craft essence with collectionNo/ID {numericIdentity}.") 
                    : Success(new CraftEssenceIdentity { CraftEssenceId = result.MstSvt.ID });
            }

            var search = craftEssenceNamingService.Namings.FuzzySearch(value.ToLowerInvariant());
            
            return Success(new CraftEssenceIdentity { CraftEssenceId = search.First().Key });
        }
        
        private static CraftEssence? ResolveNumericIdentifier(int idOrCollectionNo, MasterDataService masterDataService)
        {
            var jp = masterDataService.Connections[Region.JP];
            return jp.GetCraftEssenceById(idOrCollectionNo) ?? jp.GetCraftEssenceByCollectionNo(idOrCollectionNo);
        }
    }
}