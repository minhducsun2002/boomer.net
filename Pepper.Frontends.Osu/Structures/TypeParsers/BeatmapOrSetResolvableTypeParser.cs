using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Text;
using Pepper.Commons.Extensions;
using Pepper.Frontends.Osu.Services;
using Pepper.Frontends.Osu.Structures.ParameterAttributes;
using Pepper.Frontends.Osu.Utils;
using Qmmands;

namespace Pepper.Frontends.Osu.Structures.TypeParsers
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
        public override async ValueTask<ITypeParserResult<IBeatmapResolvable>> ParseAsync(IDiscordCommandContext context, IParameter parameter, ReadOnlyMemory<char> input)
        {
            var isEmpty = input.Length == 0;
            if (isEmpty)
            {
                var doNotFill = parameter.CustomAttributes.OfType<DoNotFillAttribute>().Any();
                if (!doNotFill)
                {
                    var ctx = (IDiscordTextCommandContext) context;
                    var beatmapId = await ctx.GetBeatmapIdFromContext();
                    if (beatmapId != null)
                    {
                        return Success(new BeatmapResolvable(beatmapId.Value));
                    }
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
        public override async ValueTask<ITypeParserResult<IBeatmapOrSetResolvable>> ParseAsync(IDiscordCommandContext context, IParameter parameter, ReadOnlyMemory<char> input)
        {
            var isEmpty = input.Length == 0;
            if (isEmpty)
            {
                var ctx = (IDiscordTextCommandContext) context;
                var beatmapId = await ctx.GetBeatmapIdFromContext();
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