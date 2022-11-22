using System.Diagnostics;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Text;
using Qmmands;
using Qmmands.Text;

namespace Pepper.Commons.Commands.Debugging
{
    public class Time : DebuggingCommand
    {
        [TextCommand("time")]
        [Description("Measure the time taken executing a command.")]
        public async Task<IDiscordCommandResult?> Exec(
            [Remainder][Description("Command to execute.")] string input
        )
        {
            var commandService = Context.GetCommandService();
            var prefixes = await Bot.Prefixes.GetPrefixesAsync(Context.Message);

            var foundPrefix = prefixes!.FirstOrDefault(
                prefix => input.StartsWith(prefix.ToString()!, StringComparison.OrdinalIgnoreCase)
            );
            if (foundPrefix == null)
            {
                return Reply("Prefix mismatch - don't think execution is possible.");
            }

            var memory = input.AsMemory();
            var context = Bot.CreateTextCommandContext(
                foundPrefix,
                memory[foundPrefix.ToString()!.Length..],
                Context.Message,
                Context is IDiscordTextGuildCommandContext guildContext ? guildContext.Channel : null
            );

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var r = await commandService.ExecuteAsync(context);
            stopwatch.Stop();
            await Reply($"Executed your snippet in {stopwatch.ElapsedMilliseconds}ms.");
            return r as IDiscordCommandResult;
        }
    }
}