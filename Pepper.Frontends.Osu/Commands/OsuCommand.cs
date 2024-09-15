using System.Globalization;
using AJ.Code;
using Disqord;
using Disqord.Rest;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using Pepper.Commons.Osu;
using Pepper.Commons.Structures;
using Pepper.Commons.Structures.CommandAttributes.Metadata;
using Pepper.Frontends.Osu.Structures.TypeParsers;

namespace Pepper.Frontends.Osu.Commands
{
    [Category("osu!")]
    public abstract class OsuCommand : Command
    {
        protected OsuCommand(APIClientStore apiClientStore) => APIClientStore = apiClientStore;
        protected readonly APIClientStore APIClientStore;
        protected static Ruleset[] Rulesets => RulesetTypeParser.SupportedRulesets;
        // ReSharper disable once InconsistentNaming
        protected const GameServer server = GameServer.Osu;

        private static readonly LocalEmoji Hourglass = new("⏳");
        public override async ValueTask OnBeforeExecuted()
        {
            await Context.Message.AddReactionAsync(Hourglass);
        }

        public override async ValueTask OnAfterExecuted()
        {
            await Context.Message.RemoveOwnReactionAsync(Hourglass);
        }

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

        protected static string SerializeHitStats(Dictionary<HitResult, int> statistics, RulesetInfo rulesetInfo, Dictionary<HitResult, int>? maximumStatistics = null)
        {
            var sc = new ScoreInfo
            {
                Ruleset = rulesetInfo,
                Statistics = statistics
            };

            var stats = sc.GetStatisticsForDisplay().ToList();
            if (rulesetInfo.OnlineID == 2) // osu!catch
            {
                var great = statistics.GetValueOrDefault(HitResult.Great, 0);
                var ticks = statistics.GetValueOrDefault(HitResult.LargeTickHit, 0);
                
                var maximumTicks = maximumStatistics?.GetValueOrDefault(HitResult.LargeTickHit, 0);
                
                var tickMiss = statistics.GetValueOrDefault(HitResult.SmallTickMiss, 0);
                var miss = statistics.GetValueOrDefault(HitResult.Miss, 0);
                
                stats = new List<HitResultDisplayStatistic>
                {
                    new(HitResult.Great, great, new int?(), nameof(HitResult.Great)),
                    new(HitResult.LargeTickHit, ticks, maximumTicks, nameof(HitResult.LargeTickHit)),
                    new(HitResult.SmallTickMiss, tickMiss, new int?(), nameof(HitResult.SmallTickMiss)),
                    new(HitResult.Miss, miss, new int?(), nameof(HitResult.Miss)),
                };
            }
            
            var displayStats = stats
                .Select(s => s.Result)
                .Where(r => !r.IsBonus())
                .Select(hitResult => $"**{(sc.Statistics.TryGetValue(hitResult, out var value) ? value : 0)}**");
            return string.Join('/', displayStats);
        }

        public static string SerializeTimestamp(DateTimeOffset? timestamp)
            => $"<t:{(timestamp ?? DateTimeOffset.UnixEpoch).ToUnixTimeSeconds()}:R>";


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

        protected static string SerializeMods(IReadOnlyList<APIMod> mods)
        {
            if (mods.Count == 1 && mods.ElementAtOrDefault(0)?.Acronym == "CL")
            {
                return "";
            }

            var m = mods.Where(m => m.Acronym != "CL");
            return mods.Count != 0 ? "+" + string.Join("", m.Select(mod => mod.Acronym)) : "";
        }

        protected static string SerializeMods(IReadOnlyList<Mod> mods)
        {
            if (mods.Count == 1 && mods.ElementAtOrDefault(0) is ModClassic)
            {
                return "";
            }

            var m = mods.Where(m => m is not ModClassic);
            return mods.Count != 0 ? "+" + string.Join("", m.Select(mod => mod.Acronym)) : "";
        }
    }
}