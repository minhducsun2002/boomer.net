using Pepper.Commons.Maimai.Structures.Data.Enums;

namespace Pepper.Commons.Maimai.Structures.Data.Score
{
    public class RecentRecord : ScoreRecord
    {
#pragma warning disable CS8618
        public int MultiplayerRank { get; set; }
        public int Track { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string? ImageUrl { get; set; }
        public ChallengeType ChallengeType { get; set; }
        public int ChallengeRemainingHealth { get; set; }
        public int ChallengeMaxHealth { get; set; }
        public bool? IsWinningMatching { get; set; }
#pragma warning restore CS8618
    }
}