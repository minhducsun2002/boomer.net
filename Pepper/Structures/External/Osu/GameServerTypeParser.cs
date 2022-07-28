using System;
using System.Threading.Tasks;
using Disqord.Bot;
using Disqord.Bot.Commands;
using Pepper.Commons.Osu;
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
            return Success(result);
        }
    }
}