namespace Pepper.Commons.Maimai.Structures
{
    public class ScoreRecord
    {
#pragma warning disable CS8618
        public ChartVersion Version { get; set; }
        public string Name { get; set; }
        public string Rank { get; set; }
        public int Accuracy { get; set; }
        public FcStatus FcStatus { get; set; }
        public SyncStatus SyncStatus { get; set; }
        public Difficulty Difficulty { get; set; }
        public (int, bool) Level { get; set; }
        public int Notes { get; set; }
        public int MaxNotes { get; set; }
#pragma warning restore CS8618
    }
}