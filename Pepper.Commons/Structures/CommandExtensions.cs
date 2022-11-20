using Disqord.Bot;
using Disqord.Bot.Commands.Text;
using Qmmands.Text;

namespace Pepper.Commons.Structures
{
    public static class CommandExtensions
    {
        public static string[] GetPrefixes(this ITextCommand command, DiscordBotBase bot)
        {
            var prefixes = ((DefaultPrefixProvider) bot.Prefixes).Prefixes.OfType<StringPrefix>().Select(prefix => prefix.ToString());
            return prefixes.ToArray();
        }

        public static string GetPrimaryInvocation(this ITextCommand command, DiscordBotBase bot)
        {
            var prefix = ((DefaultPrefixProvider) bot.Prefixes).Prefixes
                .OfType<StringPrefix>()
                .Select(prefix => prefix.ToString());
            var name = command.Aliases.MinBy(s => s.Length);
            return prefix + name;
        }
    }
}