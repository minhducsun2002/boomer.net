namespace Pepper.Commons.Maimai.Structures.Data
{
    public class User : SimpleUser
    {
#pragma warning disable CS8618
        public int PlayCount { get; init; }
        public UserStatistics UserStatistics { get; init; }
#pragma warning restore CS8618
    }
}