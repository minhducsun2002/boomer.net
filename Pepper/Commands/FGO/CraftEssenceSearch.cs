using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Humanizer;
using Pepper.FuzzySearch;
using Pepper.Services.FGO;
using Pepper.Structures;
using Pepper.Structures.External.FGO;
using Pepper.Structures.External.FGO.Entities;
using Qmmands;
using LocalEmbed = Disqord.LocalEmbed;
using LocalEmbedField = Disqord.LocalEmbedField;
using PagedView = Pepper.Structures.PagedView;

namespace Pepper.Commands.FGO
{
    public class CraftEssenceSearch : FGOCommand
    {
        private readonly CraftEssenceNamingService ceNamingService;
        public CraftEssenceSearch(CraftEssenceNamingService craftEssenceNamingService)
            => ceNamingService = craftEssenceNamingService;
        
        public async Task<DiscordCommandResult> Exec(
            [Description("A CE name, ID, or collectionNo.")] string query
        )
        {
            if (string.IsNullOrWhiteSpace(query)) return Reply("Please specify a query :frowning:");
            var searchResults = ceNamingService.Namings.FuzzySearch(query);
            var isOwner = await Context.Bot.IsOwnerAsync(Context.Author.Id);
            var pageProvider = new ArrayPageProvider<FuseSearchResult<NamedKeyedEntity<int,string>>>(
                searchResults,
                (_, segment) => new Page().WithEmbeds(
                    new LocalEmbed
                    {
                        Title = $"Search results for `{query}`",
                        Description = string.Join(
                            "\n ",
                            segment.Select(result =>
                                $"{ceNamingService.CEIdToCollectionNo[result.Element.Key]}. **{result.Element.Name}**"
                                + (isOwner ? $" [{result.Score:0.#}]" : "")
                            )
                        )
                    }),
                Math.Min(15, searchResults.Length)
            );
            
            return View(new PagedView(pageProvider));
        }
    }
}