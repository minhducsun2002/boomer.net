using Pepper.Commons.Maimai.Entities;

namespace Pepper.Frontends.Maimai.Structures
{
    public static class DifficultyExtensions
    {
        public static ChartLevel ExtractLevel(this Difficulty d)
        {
            return new ChartLevel { Whole = d.Level, Decimal = d.LevelDecimal };
        }
    }
}