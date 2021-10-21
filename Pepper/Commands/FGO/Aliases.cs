using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Services.FGO;
using Pepper.Structures.Commands;
using Pepper.Structures.External.FGO.Entities;
using Pepper.Structures.External.FGO.TypeParsers;
using Qmmands;

namespace Pepper.Commands.FGO
{
    internal class ServantResolvableFailureFormatterAttribute : Attribute, IParameterCheckFailureFormatter
    {
        public LocalMessage? FormatFailure(ParameterChecksFailedResult parameterChecksFailedResult)
            => new LocalMessage().WithContent("Please specify a valid servant alias or ID.");
    }

    internal class ServantResolvableCheckAttribute : ParameterCheckAttribute
    {
        public override ValueTask<CheckResult> CheckAsync(object argument, CommandContext context)
        {
            var typeParser = (ServantIdentityTypeParser) context.Command.Service.GetTypeParser<ServantIdentity>();
            if (argument is string maybeCollectionNo && int.TryParse(maybeCollectionNo, out var surelyCollectionNo))
            {
                argument = surelyCollectionNo;
            }

            switch (argument)
            {
                case string alias:
                    {
                        var list = typeParser.GetAlias(alias);
                        return list.Count != 0
                            ? Success()
                            : Failure("Alias does not resolve to an existent servant.");
                    }
                case int collectionNo:
                    {
                        var masterDataService = context.Services.GetRequiredService<MasterDataService>();
                        var servantEntity = masterDataService.Connections[Region.JP].GetServantEntityByCollectionNo(collectionNo);
                        return servantEntity != null
                            ? Success()
                            : Failure("collectionNo does not map to an existent servant.");
                    }
                default:
                    return Failure("");
            }
        }
    }

    public class Aliases : FGOCommand
    {
        private readonly ServantNamingService servantNamingService;
        private readonly ServantIdentityTypeParser servantIdentityTypeParser;

        public Aliases(ServantNamingService s, CommandService commandService)
        {
            servantNamingService = s;
            servantIdentityTypeParser = (ServantIdentityTypeParser) commandService.GetTypeParser<ServantIdentity>();
        }

        private string Name(int collectionNo)
        {
            var (key, _) = servantNamingService.ServantIdToCollectionNo.FirstOrDefault(kv => kv.Value == collectionNo);
            return servantNamingService.Namings.ContainsKey(key)
                ? $"**{servantNamingService.Namings[key].Name}** ({collectionNo})"
                : $"{collectionNo}";
        }


        [Command("addname")]
        [Description("Add an alias to a servant.")]
        public DiscordCommandResult Add(
            [Description("Servant collectionNo to add an alias for.")][ServantResolvableCheck] int collectionNo,
            [Description("Alias to add.")] string alias)
        {
            if (servantIdentityTypeParser.GetAlias(alias).Any())
            {
                return Reply($"`{alias}` is already mapped to servant {Name(collectionNo)}.");
            }

            servantIdentityTypeParser.AddAlias(alias, collectionNo, Context.Author.Id.ToString());
            return Reply($"Mapped `{alias}` to servant {Name(collectionNo)}.");
        }

        [Command("getnames", "getname")]
        [Description("List aliases of a servant.")]
        public DiscordCommandResult Get(
            [Description("Servant alias/collectionNo to list aliases for.")][ServantResolvableCheck][ServantResolvableFailureFormatter] string alias
        )
        {
            var list = int.TryParse(alias, out var collectionNo) switch
            {
                false => servantIdentityTypeParser.GetAlias(alias),
                true => servantIdentityTypeParser.GetAlias(collectionNo)
            };

            return Reply(new LocalEmbed
            {
                Title = $"Custom {(list.Count > 1 ? "alias".Pluralize() : "alias")} for servant {Name(collectionNo)}",
                Description = string.Join("\n", list.Select(res => $"- `{res.Alias}`"))
            });
        }
    }
}