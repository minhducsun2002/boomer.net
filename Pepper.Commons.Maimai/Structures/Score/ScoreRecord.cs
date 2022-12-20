using Pepper.Commons.Maimai.Structures.Enums;

namespace Pepper.Commons.Maimai.Structures.Score
{
    public abstract class ScoreRecord
    {
#pragma warning disable CS8618
        public ChartVersion Version { get; set; }
        public string Name { get; set; }
        public string Rank { get; set; }
        public bool RankPlus { get; set; }
        public int Accuracy { get; set; }
        public FcStatus FcStatus { get; set; }
        public SyncStatus SyncStatus { get; set; }
        public Difficulty Difficulty { get; set; }
#pragma warning restore CS8618
    }
}