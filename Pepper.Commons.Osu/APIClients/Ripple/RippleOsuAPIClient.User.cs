using System;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using osu.Game.Rulesets;
using osu.Game.Users;
using Pepper.Commons.Osu.API;
using Pepper.Commons.Osu.API.Ripple;
using UserStatistics = osu.Game.Users.UserStatistics;

namespace Pepper.Commons.Osu.APIClients.Ripple
{
    public partial class RippleOsuAPIClient
    {
        private static APIUser DeserializeUserObject(string raw, RulesetInfo rulesetInfo)
        {
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
                    PlayTime = stats.PlayTimeInSeconds,
                    PP = stats.PerformancePoints,
                    RankedScore = stats.RankedScore,
                    ReplaysWatched = stats.ReplaysWatched,
                    TotalScore = stats.TotalScore,
                    TotalHits = stats.TotalHits
                }
            };
        }

        public override async Task<APIUser> GetUser(string username, RulesetInfo rulesetInfo)
        {
            var raw = await HttpClient.GetStringAsync(
                $"https://ripple.moe/api/v1/users/full?name={HttpUtility.UrlEncode(username)}"
            );

            return DeserializeUserObject(raw, rulesetInfo);
        }

        public async Task<APIUser> GetUser(int id, RulesetInfo rulesetInfo)
        {
            var raw = await HttpClient.GetStringAsync($"https://ripple.moe/api/v1/users/full?id={id}");
            return DeserializeUserObject(raw, rulesetInfo);
        }
    }
}