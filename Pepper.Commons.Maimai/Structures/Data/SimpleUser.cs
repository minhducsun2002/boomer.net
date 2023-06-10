using Pepper.Commons.Maimai.Structures.Data.Enums;

namespace Pepper.Commons.Maimai.Structures.Data
{
    public class SimpleUser
    {
#pragma warning disable CS8618
        public string Name { get; init; }
        public int Rating { get; init; }
        public int DanLevel { get; init; }
        public string Avatar { get; init; }
        public int StarCount { get; init; }
        public SeasonClass SeasonClass { get; init; }
#pragma warning restore CS8618
    }
}