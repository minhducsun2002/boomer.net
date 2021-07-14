using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord.Bot;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Pepper.Structures.Commands
{
    public class PrefixCheckAttribute : DiscordCheckAttribute
    {
        public override ValueTask<CheckResult> CheckAsync(DiscordCommandContext context)
        {
            var command = context.Command;
            var category = command.Module.Attributes.OfType<PrefixCategoryAttribute>().FirstOrDefault();
            if (category == null) return Success();
            
            var prefix = context.Prefix.ToString();
            var config = context.Services.GetRequiredService<IConfiguration>();
            var prefixes = config.GetCommandPrefixes(category.PrefixCategory);
            if (prefixes.Length == 0) return Success();
            return prefixes.Contains(prefix, StringComparer.InvariantCultureIgnoreCase)
                ? Success()
                : Failure($"Command {context.Command.Name} cannot be invoked with prefix {prefix}");
        }
    }
}