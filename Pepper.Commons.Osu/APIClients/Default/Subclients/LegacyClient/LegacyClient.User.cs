using System;
using System.Threading;
using System.Threading.Tasks;
using osu.Game.Rulesets;
using osu.Game.Users;
using OsuSharp;
using Pepper.Commons.Osu.API;
using Pepper.Commons.Osu.Exceptions;

namespace Pepper.Commons.Osu.APIClients.Default.Subclients
{
    internal partial class LegacyClient
    {
        private static APIUser ConvertFromLegacyUser(string username, User? user)
        {
            if (user == null)
            {
                throw new UserNotFoundException { Username = username };
            }

            return new APIUser
            {
                Id = (int) user.UserId,
                Username = user.Username,
                JoinDate = user.JoinDate!.Value,
                Country = new Country
                {
                    FlagName = user.Country.TwoLetterISORegionName,
                    FullName = user.Country.EnglishName
                },
                Statistics = new UserStatistics
                {
                    GlobalRank = (int) user.Rank!.Value,
                    CountryRank = (int) user.CountryRank!.Value,
                    Accuracy = user.Accuracy!.Value,
                    GradesCount = new UserStatistics.Grades
                    {
                        A = (int) user.CountA!,
                        S = (int) user.CountS!,
                        SPlus = (int) user.CountSH!,
                        SS = (int) user.CountSS!,
                        SSPlus = (int) user.CountSSH!
                    },
                    Level = new UserStatistics.LevelInfo
                    {
                        Current = (int) Math.Truncate((double) user.Level!),
                        Progress = (int) Math.Round(
                            (user.Level.Value - Math.Truncate((double) user.Level!)) * 100
                        )
                    },
                    PlayCount = (int) user.PlayCount!,
                    PlayTime = (int?) user.TimePlayed.TotalSeconds,
                    PP = (decimal?) user.PerformancePoints,
                    RankedScore = (long) user.RankedScore!,
                    ReplaysWatched = 0,
                    TotalScore = (long) user.Score!,
                    TotalHits = (int) (user.Count300 + user.Count100 + user.Count50)!
                }
            };
        }

        public async Task<APIUser> GetUserAsync(string username, RulesetInfo rulesetInfo, CancellationToken cancellationToken)
        {
            var user = await osuClient.GetUserByUsernameAsync(username, (OsuSharp.GameMode) rulesetInfo.OnlineID, cancellationToken);

            return ConvertFromLegacyUser(username, user);
        }

        public async Task<APIUser> GetUserAsync(int userId, RulesetInfo rulesetInfo, CancellationToken cancellationToken)
        {
            var user = await osuClient.GetUserByUserIdAsync(userId, (OsuSharp.GameMode) rulesetInfo.OnlineID, cancellationToken);

            return ConvertFromLegacyUser(userId.ToString(), user);
        }
    }
}