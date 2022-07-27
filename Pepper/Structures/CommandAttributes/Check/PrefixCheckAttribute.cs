using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord.Bot;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Structures.CommandAttributes.Metadata;
using Qmmands;

namespace Pepper.Structures.CommandAttributes.Checks
{
    public class PrefixCheckAttribute : DiscordCheckAttribute
    {
        public override ValueTask<IResult> CheckAsync(IDiscordCommandContext ctx)
        {
            if (ctx is not IDiscordTextCommandContext context)
            {
                return Results.Success;
            }

            var command = context.Command;
            var category = command?.Module.CustomAttributes.OfType<PrefixCategoryAttribute>().FirstOrDefault();
            if (category == null)
            {
                return Results.Success;
            }

            var prefix = context.Prefix.ToString();
            var config = context.Services.GetRequiredService<IConfiguration>();
            var prefixes = config.GetCommandPrefixes(category.PrefixCategory);
            if (prefixes.Length == 0)
            {
                return Results.Success;
            }

            return prefixes.Contains(prefix, StringComparer.InvariantCultureIgnoreCase)
                ? Results.Success
                : Results.Failure($"Command {context.Command?.Name ?? "<no command>"} cannot be invoked with prefix {prefix}");
        }

    }
}