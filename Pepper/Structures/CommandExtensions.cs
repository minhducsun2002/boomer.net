using System.Linq;
using Disqord.Bot;
using Disqord.Bot.Commands.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Structures.Commands;
using Qmmands.Text;

namespace Pepper.Structures
{
    public static class CommandExtensions
    {
        public static string[] GetPrefixes(this ITextCommand command, DiscordBotBase bot)
        {
            var prefixCategory = command.CustomAttributes.OfType<PrefixCategoryAttribute>().FirstOrDefault()?.PrefixCategory;
            var config = bot.Services.GetRequiredService<IConfiguration>();
            var prefixes = string.IsNullOrWhiteSpace(prefixCategory)
                ? ((DefaultPrefixProvider) bot.Prefixes).Prefixes.OfType<StringPrefix>().Select(prefix => prefix.ToString())
                : config.GetCommandPrefixes(prefixCategory);
            return prefixes.ToArray();
        }
    }
}