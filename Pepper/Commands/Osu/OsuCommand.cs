using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using AJ.Code;
using Disqord;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch.Difficulty;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Difficulty;
using osu.Game.Scoring;
using Pepper.Commons.Osu;
using Pepper.Structures;
using Pepper.Structures.CommandAttributes.Metadata;
using Pepper.Structures.External.Osu;
using Pepper.Structures.External.Osu.Extensions;
using APIBeatmapSet = Pepper.Commons.Osu.API.APIBeatmapSet;

namespace Pepper.Commands.Osu
{
    [PrefixCategory("osu")]
    [Category("osu!")]
    public abstract class OsuCommand : Command
    {
        public OsuCommand(APIClientStore apiClientStore) => APIClientStore = apiClientStore;
        protected readonly APIClientStore APIClientStore;
        protected static Ruleset[] Rulesets => RulesetTypeParser.SupportedRulesets;

        protected static string ResolveEarthEmoji(string countryCode)
        {
            var continent = Country.GetCountryInfoForAlpha2Code(countryCode)?.ContinentCode;
            return continent switch
            {
                ContinentCode.AF => ":earth_africa:",
                ContinentCode.EU => ":earth_africa:",
                ContinentCode.NA => ":earth_americas:",
                ContinentCode.SA => ":earth_americas:",
                _ => ":earth_asia:"
            };
        }

        public static string SerializeBeatmapStats(
            IBeatmapInfo map,
            IEnumerable<Mod>? mods = null,
            DifficultyAttributes? difficultyOverwrite = null,
            ControlPointInfo? controlPointInfo = null,
            bool showLength = true, char delimiter = '•')
        {
            return new BeatmapStatsSerializer(map)
            {
                Mods = mods,
                ControlPointInfo = controlPointInfo,
                DifficultyOverwrite = difficultyOverwrite
            }.Serialize(
                formatted: true,
                serializationOptions: BeatmapSerializationOptions.Statistics |
                                      BeatmapSerializationOptions.BPM |
                                      BeatmapSerializationOptions.StarRating |
                                      (showLength ? BeatmapSerializationOptions.Length : 0)
            );
        }

        public static string SerializeBeatmapStats(
            APIBeatmap diff,
            bool formatted,
            APIBeatmapSet? beatmapset = null,
            string delimiter = " • ",
            BeatmapSerializationOptions serializationOptions = BeatmapSerializationOptions.Length |
                                                               BeatmapSerializationOptions.Statistics |
                                                               BeatmapSerializationOptions.BPM |
                                                               BeatmapSerializationOptions.StarRating
        )
        {
            return new BeatmapStatsSerializer(diff)
            {
                ControlPointInfo = null,
                DifficultyOverwrite = null,
                Mods = null
            }.Serialize(formatted, serializationOptions: serializationOptions);
        }

        protected static string SerializeHitStats(Dictionary<string, int> statistics, RulesetInfo rulesetInfo)
        {
            var sc = new ScoreInfo
            {
                Ruleset = rulesetInfo
            }.WithStatistics(statistics);
            var displayStats = sc.GetStatisticsForDisplay()
                .Select(s => s.Result)
                .Where(r => !r.IsBonus())
                .Select(hitResult => $"**{(sc.Statistics.TryGetValue(hitResult, out var value) ? value : 0)}**");
            return string.Join('/', displayStats);
        }

        public static string SerializeTimestamp(DateTimeOffset? timestamp, bool utcHint = true)
            => (timestamp ?? DateTimeOffset.UnixEpoch)
                .ToUniversalTime()
                .ToString($"HH:mm:ss, dd/MM/yyyy{(utcHint ? " 'UTC'" : "")}", CultureInfo.InvariantCulture);

        public static LocalEmbedAuthor SerializeAuthorBuilder(APIUser user)
        {
            var avatarUrl = user.AvatarUrl;
            var userUrl = $"https://osu.ppy.sh/users/{user.Id}";
            if (user is Commons.Osu.API.APIUser overriddenUserInstance)
            {
                avatarUrl = overriddenUserInstance.AvatarUrl;
                userUrl = overriddenUserInstance.PublicUrl;
            }
            var embedAuthor = new LocalEmbedAuthor
            {
                IconUrl = avatarUrl,
                Name = $"{user.Username}" + ((user.Statistics.PP ?? decimal.Zero) == decimal.Zero ? "" : $" ({user.Statistics.PP}pp)"),
                Url = userUrl
            };
            return embedAuthor;
        }
    }
}