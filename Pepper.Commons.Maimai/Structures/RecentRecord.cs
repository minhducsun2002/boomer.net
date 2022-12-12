namespace Pepper.Commons.Maimai.Structures
{
    public class RecentRecord
    {
#pragma warning disable CS8618
        public ChartVersion Version { get; set; }
        public string Name { get; set; }
        public string Rank { get; set; }
        public int MultiplayerRank { get; set; }
        public int Accuracy { get; set; }
        public int Track { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public FcStatus FcStatus { get; set; }
        public SyncStatus SyncStatus { get; set; }
        public Difficulty Difficulty { get; set; }
        public string? ImageUrl { get; set; }
        public ChallengeType ChallengeType { get; set; }
        public int ChallengeRemainingHealth { get; set; }
        public int ChallengeMaxHealth { get; set; }
        public bool? IsWinningMatching { get; set; }
#pragma warning restore CS8618
    }
}