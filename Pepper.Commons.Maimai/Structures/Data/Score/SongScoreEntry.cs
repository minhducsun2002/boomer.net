using Pepper.Commons.Maimai.Structures.Data.Enums;

namespace Pepper.Commons.Maimai.Structures.Data.Score
{
    public class SongScoreEntry
    {
        public ChartVersion Version { get; set; }
        public string Name { get; set; }
        public Difficulty Difficulty { get; set; }
        public string? MusicDetailLink { get; set; }
        public (int, bool) Level { get; set; }
    }
}