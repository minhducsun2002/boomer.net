using System.Collections.Generic;
using System.Linq;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus.Paged;
using FuzzySharp;
using Pepper.Services.FGO;
using Qmmands;
using PagedView = Pepper.Structures.PagedView;

namespace Pepper.Commands.FGO
{
    public class Traits : FGODataCommand
    {
        public Traits(MasterDataService m, TraitService t) : base(m, t) {}

        [Command("trait", "traits", "t")]
        [Description("List known traits, or search for one.")]
        public DiscordCommandResult Exec([Description("Search query if any.")] string query = "")
        {
            var traits = TraitService.Traits;
            var listing = traits.OrderBy(kv => kv.Key).ToArray();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var res = Process.ExtractSorted(
                    new KeyValuePair<int, string>(default, query),
                    listing,
                    kv => kv.Value,
                    null
                );
                listing = res
                    .Select(result => result.Value).ToArray();
            }
            
            var pageProvider = new ArrayPageProvider<KeyValuePair<int, string>>(
                listing,
                (_, segment) => new Page().WithEmbeds(new LocalEmbed
                {
                    Title = string.IsNullOrWhiteSpace(query) ? "Known traits" : $"Trait search results for `{query}`",
                    Description = string.Join("\n", segment.Select(kv => $"`{kv.Key}` {kv.Value}"))
                }),
                20
            );

            return View(new PagedView(pageProvider));
        }
    }
}