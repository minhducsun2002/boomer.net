using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;

namespace Pepper.Frontends.Osu.Structures.Extensions
{
    public static class ScoreInfoExtensions
    {
        public static ScoreInfo WithRulesetID(this ScoreInfo scoreInfo, int rulesetId)
        {
            scoreInfo.Ruleset.OnlineID = rulesetId;
            return scoreInfo;
        }

        public static ScoreInfo WithStatistics(this ScoreInfo scoreInfo, Dictionary<string, int> statistics)
        {
            foreach (var (key, value) in statistics)
            {
                switch (key)
                {
                    case @"count_geki": scoreInfo.SetCountGeki(value); break;
                    case @"count_300": scoreInfo.SetCount300(value); break;
                    case @"count_katu": scoreInfo.SetCountKatu(value); break;
                    case @"count_100": scoreInfo.SetCount100(value); break;
                    case @"count_50": scoreInfo.SetCount50(value); break;
                    case @"count_miss": scoreInfo.SetCountMiss(value); break;
                }
            }

            return scoreInfo;
        }
    }
}