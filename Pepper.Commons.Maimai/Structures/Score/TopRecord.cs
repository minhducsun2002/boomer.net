namespace Pepper.Commons.Maimai.Structures.Score
{
    public class TopRecord : ScoreRecord
    {
        public (int, bool) Level { get; set; }
        public int Notes { get; set; }
        public int MaxNotes { get; set; }
    }
}