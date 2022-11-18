using System;
using System.Collections.Generic;
using System.Linq;
using Qmmands;
using Qmmands.Text;

namespace Pepper.Commons.Structures
{
    public static class CommandServiceExtensions
    {
        public static IEnumerable<ITextCommand> EnumerateTextCommands(this ICommandService commandService)
        {
            return commandService.EnumerateTextModules()
                .SelectMany(CommandUtilities.EnumerateAllCommands)
                .OfType<ITextCommand>();
        }

        public static IList<ITextCommandMatch> FindCommands(this ICommandService commandService, ReadOnlyMemory<char> input)
        {
            return commandService
                .GetCommandMapProvider()
                .GetRequiredMap<ITextCommandMap>()
                .FindMatches(input);
        }
    }
}