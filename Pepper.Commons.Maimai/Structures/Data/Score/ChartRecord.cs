namespace Pepper.Commons.Maimai.Structures.Data.Score
{
    public class ChartRecord : TopRecord
    {
        public DateTimeOffset LastPlayed { get; set; }
        public int PlayCount { get; set; }
    }
}