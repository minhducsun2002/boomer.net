using System;

namespace Pepper.Structures.External.Osu
{
    [Flags]
    public enum StatFilter
    {
        StarRating = 1 << 1,
        Statistics = 1 << 2,
        BPM = 1 << 3,
        Length = 1 << 4,
        Combo = 1 << 5
    }
}