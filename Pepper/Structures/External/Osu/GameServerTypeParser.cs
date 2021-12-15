using System;
using System.Threading.Tasks;
using Disqord.Bot;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Commons.Osu;
using Pepper.Services;
using Qmmands;

namespace Pepper.Structures.External.Osu
{
    public class GameServerTypeParser : DiscordTypeParser<GameServer>
    {
        public override ValueTask<TypeParserResult<GameServer>> ParseAsync(Parameter parameter, string value, DiscordCommandContext context)
        {
            GameServer result;
            try
            {
                result = Enum.Parse<GameServer>(value, true);
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