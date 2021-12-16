using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Web;
using BitFaster.Caching.Lru;
using Newtonsoft.Json;
using osu.Game.Rulesets;
using osu.Game.Users;
using OsuSharp;
using Pepper.Commons.Osu.API;
using Pepper.Commons.Osu.API.Ripple;

namespace Pepper.Commons.Osu.APIClients.Ripple
{
    public partial class RippleOsuAPIClient
    {
        private readonly FastConcurrentTLru<string, Color> userColorCache = new(200, TimeSpan.FromSeconds(30 * 60));

        public override async Task<APIUser> GetUser(string username, RulesetInfo rulesetInfo)
        {
            var raw = await HttpClient.GetStringAsync(
                $"https://ripple.moe/api/v1/users/full?name={HttpUtility.UrlEncode(username)}"
            );

            var user = JsonConvert.DeserializeObject<RippleUser>(raw)!;
            var stats = user.Statistics[(GameMode) rulesetInfo.OnlineID];
            return new RippleAPIUser
            {
                Id = user.UserId,
                Username = user.Username,
                JoinDate = user.RegisteredOn,
                LastVisit = user.LastActive,
                Country = new Country
                {
                    FlagName = user.Country.TwoLetterISORegionName,
                    FullName = user.Country.EnglishName
                },
                Statistics = new UserStatistics
                {
                    GlobalRank = stats.GlobalLeaderboardRank,
                    CountryRank = stats.CountryLeaderboardRank,
                    Accuracy = stats.Accuracy,
                    GradesCount = new UserStatistics.Grades
                    {
                        A = 0,
                        S = 0,
                        SPlus = 0,
                        SS = 0,
                        SSPlus = 0
                    },
                    Level = new UserStatistics.LevelInfo
                    {
                        Current = (int) Math.Truncate(stats.Level),
                        Progress = (int) Math.Truncate(stats.Level * 100) % 100
                    },
                    PlayCount = stats.PlayCount,
                    PlayTime = (int) TimeSpan.FromSeconds(stats.PlayTimeInSeconds).Ticks / (int) 1e7,
                    PP = stats.PerformancePoints,
                    RankedScore = stats.RankedScore,
                    ReplaysWatched = stats.ReplaysWatched,
                    TotalScore = stats.TotalScore,
                    TotalHits = stats.TotalHits
                }
            };
        }
    }
}