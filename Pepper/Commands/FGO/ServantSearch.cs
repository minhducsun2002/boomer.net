using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Pepper.Services.FGO;
using Pepper.Structures.External.FGO;
using Qmmands;
using PagedView = Pepper.Structures.PagedView;

namespace Pepper.Commands.FGO
{
    public class ServantSearch : FGOCommand
    {
        private readonly ServantNamingService namingService;
        public ServantSearch(ServantNamingService namingService) => this.namingService = namingService;

        [Command("ss", "ssno", "ssn", "search-servants-name")]
        [Description("Search for servants,")]
        public async Task<DiscordCommandResult> Exec(
            [Description("Search query.")] string query = ""
            // [Description("Traits")] params string[] traits
        )
        {
            if (string.IsNullOrWhiteSpace(query)) return Reply("Please specify a query :frowning:");
            
            var servantIdentityTypeParser = (ServantIdentityTypeParser) Context.Bot.Commands.GetTypeParser<ServantIdentity>();
            var collectionNoLookup = namingService.ServantIdToCollectionNo;

            var searchResults = servantIdentityTypeParser.Search(query, Context.Services);
            var isOwner = await Context.Bot.IsOwnerAsync(Context.Author.Id);
            var pageProvider = new ArrayPageProvider<ServantSearchRecord>(
                searchResults,
                (_, segment) => new Page().WithEmbeds(
                    new LocalEmbed
                    {
                        Title = $"Search results for `{query}`",
                        Description = string.Join(
                            "\n",
                            segment.Select(result =>
                                $"{collectionNoLookup[result.ServantId]}. **{result.Name}**"
                                + (isOwner ? $" [{result.Score:0.#}, {result.Bucket}]" : "")
                            )
                        )
                    }
                ),
                15
            );

            return View(new PagedView(pageProvider));
        }
    }
}