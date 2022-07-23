using System;
using System.Threading.Tasks;
using Disqord.Bot;
using Disqord.Bot.Commands;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Commons.Osu;
using Pepper.Services;
using Qmmands;

namespace Pepper.Structures.External.Osu
{
    public class GameServerTypeParser : DiscordTypeParser<GameServer>
    {
        public override ValueTask<ITypeParserResult<GameServer>> ParseAsync(IDiscordCommandContext context, IParameter parameter, ReadOnlyMemory<char> input)
        {
            GameServer result;
            try
            {
                result = Enum.Parse<GameServer>(input.Span, true);
            }
            catch
            {
                result = GameServer.Osu;
            }
            var s = context.Services.GetRequiredService<TypeParsedArgumentPersistenceService>();
            s.Set(result);
            return Success(result);
        }
    }
}