namespace Pepper.Commons.Maimai.Structures.Score
{
    public class TopRecord : ScoreRecord
    {
        public (int, bool) Level { get; set; }
        public int Notes { get; set; }
        public int MaxNotes { get; set; }
        // append this to https://maimaidx-eng.com/maimai-mobile/record/musicDetail/?idx=
        // to get working link
        public string? MusicDetailLink { get; set; }
    }
}