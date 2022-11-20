using Disqord.Bot.Commands;
using Pepper.Frontends.Osu.Services;
using Pepper.Frontends.Osu.Utils;
using Qmmands;

namespace Pepper.Frontends.Osu.Structures
{
    public interface IBeatmapOrSetResolvable { }
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
        private readonly BeatmapContextProviderService contextProviderService;
        public BeatmapResolvableTypeParser(BeatmapContextProviderService contextProvider)
        {
            contextProviderService = contextProvider;
        }

        public override ValueTask<ITypeParserResult<IBeatmapResolvable>> ParseAsync(IDiscordCommandContext context, IParameter parameter, ReadOnlyMemory<char> input)
        {
            var isEmpty = input.Length == 0;
            if (isEmpty)
            {
                var beatmapId = contextProviderService.GetBeatmap(context.ChannelId.ToString());
                if (beatmapId != null)
                {
                    return Success(new BeatmapResolvable(beatmapId.Value));
                }
            }

            var value = input.ToString();
            if (URLParser.CheckMapUrl(value, out _, out var id, out _) && id != null)
            {
                return Success(new BeatmapResolvable(id.Value));
            }

            return int.TryParse(value, out var targetId)
                ? Success(new BeatmapResolvable(targetId))
                : Failure(
                    isEmpty
                    ? "No beatmap is specified, and there was no previous beatmap/score viewed in this channel."
                    : "Could not determine a beatmap ID from given input."
                );
        }
    }

    public class BeatmapOrSetResolvableTypeParser : DiscordTypeParser<IBeatmapOrSetResolvable>
    {
        private readonly BeatmapContextProviderService contextProviderService;
        public BeatmapOrSetResolvableTypeParser(BeatmapContextProviderService contextProvider)
        {
            contextProviderService = contextProvider;
        }

        public override ValueTask<ITypeParserResult<IBeatmapOrSetResolvable>> ParseAsync(IDiscordCommandContext context, IParameter parameter, ReadOnlyMemory<char> input)
        {
            var isEmpty = input.Length == 0;
            if (isEmpty)
            {
                var beatmapId = contextProviderService.GetBeatmap(context.ChannelId.ToString());
                if (beatmapId != null)
                {
                    return Success(new BeatmapResolvable(beatmapId.Value));
                }
            }

            var value = input.ToString();
            if (URLParser.CheckMapUrl(value, out _, out var id, out var setId))
            {
                if (setId != null)
                {
                    return id != null
                        ? Success(new BeatmapAndSetResolvable(id.Value, setId.Value))
                        : Success(new BeatmapsetResolvable(setId.Value));
                }

                if (id != null)
                {
                    return Success(new BeatmapResolvable(id.Value));
                }
            }

            return int.TryParse(value, out var ambiguousId)
                ? Success(new BeatmapOrSetResolvable(ambiguousId))
                : Failure(isEmpty
                    ? "No beatmap(set) is specified, and there was no previous beatmap/score viewed in this channel."
                    : "Could not determine if this is a beatmap or beatmapset link.");
        }
    }
}