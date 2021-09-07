using System.Linq;
using Disqord;
using Disqord.Bot;
using Humanizer;
using Pepper.Services.FGO;
using Qmmands;

namespace Pepper.Commands.FGO.Names
{
    public class Aliases : FGOCommand
    {
        private readonly ServantNamingService servantNamingService;
        private readonly ServantSearchService searchService;

        public Aliases(ServantNamingService s, ServantSearchService search)
        {
            servantNamingService = s;
            searchService = search;
        }

        private string Name(int collectionNo)
        {
            var record = servantNamingService.ServantIdToCollectionNo.FirstOrDefault(kv => kv.Value == collectionNo);
            return record.Equals(default) 
                ? $"{collectionNo}"
                : $"**{servantNamingService.Namings[record.Key].Name}** ({collectionNo})";
        }
            

        [Command("addname")]
        [Description("Add an alias to a servant.")]
        public DiscordCommandResult Add(
            [Description("Servant collectionNo to add an alias for.")] int collectionNo, 
            [Description("Alias to add.")] string alias)
        {
            if (!servantNamingService.ServantIdToCollectionNo.Values.Contains(collectionNo))
                return Reply("Please specify a valid servant collectionNo.");

            if (searchService.GetAlias(alias).Any())
                return Reply($"`{alias}` is already mapped to servant {Name(collectionNo)}.");
            
            searchService.AddAlias(alias, collectionNo, Context.Author.Id.ToString());
            return Reply($"Mapped `{alias}` to servant {Name(collectionNo)}.");
        }

        [Command("getnames", "getname")]
        [Description("List aliases of a servant.")]
        public DiscordCommandResult Get([Description("Servant alias to list aliases for.")] string alias)
        {
            var list = searchService.GetAlias(alias);
            return !list.Any() 
                ? Reply($"There's no servant associated with alias `{alias}`.") 
                : Get(list[0].CollectionNo);
        }
        
        [Priority(1)]
        [Command("getnames", "getname")]
        [Description("List aliases of a servant.")]
        public DiscordCommandResult Get([Description("Servant collectionNo to list aliases for.")] int collectionNo)
        {
            if (!servantNamingService.ServantIdToCollectionNo.Values.Contains(collectionNo))
                return Reply("Please specify a valid servant collectionNo.");

            var list = searchService.GetAlias(collectionNo);
            if (!list.Any())
                return Reply($"There's no custom alias for servant {Name(collectionNo)}.");

            return Reply(new LocalEmbed
            {
                Title = $"Custom {(list.Count > 1 ? "alias".Pluralize() : "alias")} for servant {Name(collectionNo)}",
                Description = string.Join("\n", list.Select(res => $"- `{res.Alias}`"))
            });
        }
    }
}