using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Humanizer;
using Pepper.Services.FGO;
using Pepper.Structures.Commands;
using Pepper.Structures.External.FGO.Entities;
using Pepper.Structures.External.FGO.TypeParsers;
using Qmmands;
using PagedView = Pepper.Structures.PagedView;

namespace Pepper.Commands.FGO
{
    public class ServantSearch : FGOCommand
    {
        private readonly ServantNamingService namingService;
        private readonly TraitService traitService;
        private readonly ServantSearchService searchService;

        public ServantSearch(ServantNamingService namingService, TraitService traitService, ServantSearchService searchService)
        {
            this.namingService = namingService;
            this.traitService = traitService;
            this.searchService = searchService;
        }

        [Command("ss", "ssno", "ssn", "search-servants-name")]
        [Description("Search for servants,")]
        public async Task<DiscordCommandResult> Exec(
            [Description("Search query.")] string query = "",
            [Description("Traits")][Flag("-t", "-t=", "/t:", "--trait=", "/trait:", "/")] params string[] traits
        )
        {
            if (string.IsNullOrWhiteSpace(query) && traits.Length == 0)
            {
                return Reply("Please specify a query :frowning:");
            }

            var servantIdentityTypeParser = (ServantIdentityTypeParser) Context.Bot.Commands.GetTypeParser<ServantIdentity>();
            var collectionNoLookup = namingService.ServantIdToCollectionNo;

            var searchResults = servantIdentityTypeParser.Search(query, Context.Services);
            var traitNames = Array.Empty<string>();
            if (traits.Length != 0)
            {
                if (!searchService.TraitLoaded)
                {
                    return Reply("Sorry, filtering by traits is not currently available. Please wait and try again.");
                }

                var matches = traits.Select(
                    trait => traitService.FuzzySearch.Search(trait).First()
                ).ToList();

                traitNames = matches.Select(match => match.Element.Value).ToArray();
                var longTraits = matches.Select(match => match.Element.Key).ToHashSet();

                searchResults = searchResults
                    .Where(record => searchService.ServantTraits.ContainsKey(record.ServantId))
                    .Where(record => searchService.ServantTraits[record.ServantId].IsSupersetOf(longTraits))
                    .ToArray();
            }

            var isOwner = await Context.Bot.IsOwnerAsync(Context.Author.Id);
            var pageProvider = new ArrayPageProvider<ServantSearchRecord>(
                searchResults,
                (_, segment) => new Page().WithEmbeds(
                    new LocalEmbed
                    {
                        Title = $"Search results for `{query}`",
                        Description = string.Join(
                            "\n ",
                            segment.Select(result =>
                                $"{collectionNoLookup[result.ServantId]}. **{result.Name}**"
                                + (isOwner ? $" [{result.Score:E2}, {result.Bucket}]" : "")
                            )
                        ),
                        Fields = traitNames.Length == 0
                            ? new List<LocalEmbedField>()
                            : new List<LocalEmbedField>
                            {
                                new()
                                {
                                    Name = $"Queried {(traits.Length > 1 ? "trait".Pluralize() : "trait")}",
                                    Value = string.Join(", ", traitNames)
                                }
                            }
                    }
                ),
                Math.Min(15, searchResults.Length)
            );

            return View(new PagedView(pageProvider));
        }
    }
}