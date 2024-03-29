using Pepper.Commons.Maimai.Structures.Data.Enums;

namespace Pepper.Commons.Maimai.Structures.Data.Score
{
    public abstract class ScoreRecord
    {
#pragma warning disable CS8618
        public ChartVersion Version { get; set; }
        public string Name { get; set; }
        public string Rank { get; set; }
        public bool RankPlus { get; set; }
        /// <summary>
        /// Accuracy of this score : from 0 to 1000000
        /// </summary>
        public int Accuracy { get; set; }
        public FcStatus FcStatus { get; set; }
        public SyncStatus SyncStatus { get; set; }
        public Difficulty Difficulty { get; set; }
#pragma warning restore CS8618
    }
}