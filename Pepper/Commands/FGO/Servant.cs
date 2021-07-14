using System.Linq;
using System.Threading.Tasks;
using Disqord.Bot;
using Qmmands;
using MongoDB.Driver;
using Pepper.Services.FGO;
using Pepper.Structures.Commands;
using Pepper.Structures.External.FGO;
using Pepper.Structures.External.FGO.MasterData;
using Pepper.Structures.External.FGO.Renderer;

namespace Pepper.Commands.FGO
{
    public class Servant : FGOCommand
    {
        [Command("s")]
        [PrefixCategory("fgo")]
        public async Task<DiscordCommandResult> Exec(int id = 2)
        {
            MasterDataMongoDBConnection jp = MasterDataService.Connections[Region.JP];

                // data retrieval
            var svt = (await jp.MstSvt.FindAsync(
                Builders<MstSvt>.Filter.Or(
                    Builders<MstSvt>.Filter.Eq("collectionNo", id),
                    Builders<MstSvt>.Filter.Eq("baseSvtId", id)
                )
            )).First();
            
            var renderer = new ServantRenderer(svt, MasterDataService)
            {
                ServantNamingService = ServantNamingService,
                TraitService = TraitService,
                ItemNamingService = ItemNamingService
            };
            return Reply(renderer.Prepare());
        }
    }
}