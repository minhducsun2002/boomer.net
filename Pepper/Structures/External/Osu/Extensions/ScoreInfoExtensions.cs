using System.Collections.Generic;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;

namespace Pepper.Structures.External.Osu.Extensions
{
    public static class ScoreInfoExtensions
    {
        public static void SetStatistics(this ScoreInfo scoreInfo, Dictionary<string, int> Statistics)
        {
            foreach (var (key, value) in Statistics)
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
        }
    }
}