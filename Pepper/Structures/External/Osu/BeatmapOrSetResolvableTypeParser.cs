using System.Threading.Tasks;
using Disqord.Bot;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Services.Osu;
using Pepper.Utilities.Osu;
using Qmmands;

namespace Pepper.Structures.External.Osu
{
    public interface IBeatmapOrSetResolvable {}
    internal interface IBeatmapSetResolvable : IBeatmapOrSetResolvable { public int BeatmapsetId { get; } }
    public interface IBeatmapResolvable : IBeatmapOrSetResolvable { public int BeatmapId { get; } }

    public class BeatmapResolvable : IBeatmapResolvable
    {
        internal BeatmapResolvable(int beatmapId) => BeatmapId = beatmapId;
        public int BeatmapId { get; }
    }

    public class BeatmapAndSetResolvable : IBeatmapResolvable, IBeatmapSetResolvable
    {
        internal BeatmapAndSetResolvable(int beatmapId, int beatmapsetId)
        {
            BeatmapId = beatmapId;
            BeatmapsetId = beatmapsetId;
        }
        public int BeatmapId { get; }
        public int BeatmapsetId { get; }
    }
    
    public class BeatmapsetResolvable : IBeatmapSetResolvable
    {
        internal BeatmapsetResolvable(int beatmapsetId) => BeatmapsetId = beatmapsetId;
        public int BeatmapsetId { get; }
    }

    public class BeatmapOrSetResolvable : IBeatmapOrSetResolvable
    {
        internal BeatmapOrSetResolvable(int id) => BeatmapOrSetId = id;
        public int BeatmapOrSetId { get; }
    }
    
    public class BeatmapResolvableTypeParser : DiscordTypeParser<IBeatmapResolvable>
    {
        public static readonly BeatmapResolvableTypeParser Instance = new();
        
        public override ValueTask<TypeParserResult<IBeatmapResolvable>> ParseAsync(Parameter parameter, string value, DiscordCommandContext context)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                var service = context.Services.GetRequiredService<BeatmapContextProviderService>();
                var beatmapId = service.GetBeatmap(context.ChannelId.ToString());
                if (beatmapId != null) return Success(new BeatmapResolvable(beatmapId.Value));
            }
            
            if (URLParser.CheckMapUrl(value, out _, out var id, out _) && id != null)
                return Success(new BeatmapResolvable(id.Value));
            
            return int.TryParse(value, out var targetId)
                ? Success(new BeatmapResolvable(targetId))
                : Failure("Could not determine a beatmap ID from given input.");
        }
    }

    public class BeatmapOrSetResolvableTypeParser : DiscordTypeParser<IBeatmapOrSetResolvable>
    {
        public static readonly BeatmapOrSetResolvableTypeParser Instance = new();
        
        public override ValueTask<TypeParserResult<IBeatmapOrSetResolvable>> ParseAsync(Parameter parameter, string value, DiscordCommandContext context)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                var service = context.Services.GetRequiredService<BeatmapContextProviderService>();
                var beatmapId = service.GetBeatmap(context.ChannelId.ToString());
                if (beatmapId != null) return Success(new BeatmapResolvable(beatmapId.Value));
            }
            
            if (URLParser.CheckMapUrl(value, out _, out var id, out var setId))
            {
                if (setId != null)
                    return id != null
                        ? Success(new BeatmapAndSetResolvable(id.Value, setId.Value))
                        : Success(new BeatmapsetResolvable(setId.Value));
                if (id != null) return Success(new BeatmapResolvable(id.Value));
            }

            return int.TryParse(value, out var ambiguousId)
                ? Success(new BeatmapOrSetResolvable(ambiguousId))
                : Failure("Could not determine if this is a beatmap or beatmapset link.");
        }
    }
}