namespace Pepper.Commons.Maimai.Structures
{
    public class User
    {
#pragma warning disable CS8618
        public string Name { get; init; }
        public int Rating { get; init; }
        public int DanLevel { get; init; }
        public int PlayCount { get; init; }
        public string Avatar { get; init; }
        public int StarCount { get; init; }
        public SeasonClass SeasonClass { get; init; }
        public UserStatistics UserStatistics { get; init; }
#pragma warning restore CS8618
    }
}