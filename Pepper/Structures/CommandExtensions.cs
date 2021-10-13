using System.Linq;
using Disqord.Bot;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Structures.Commands;

namespace Pepper.Structures
{
    public static class CommandExtensions
    {
        public static string[] GetPrefixes(this Qmmands.Command command, DiscordBotBase bot)
        {
            var prefixCategory = command.Attributes.OfType<PrefixCategoryAttribute>().FirstOrDefault()?.PrefixCategory;
            var config = bot.Services.GetRequiredService<IConfiguration>();
            var prefixes = string.IsNullOrWhiteSpace(prefixCategory)
                ? ((DefaultPrefixProvider) bot.Prefixes).Prefixes.OfType<StringPrefix>().Select(prefix => prefix.ToString())
                : config.GetCommandPrefixes(prefixCategory);
            return prefixes.ToArray();
        }
    }
}