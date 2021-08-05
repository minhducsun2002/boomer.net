using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pepper.Utilities.Osu
{
    public static partial class URLParser
    {
        private static readonly Regex BeatmapOrBeatmapsetRegex = new (
            @"http(?:s)*:\/\/osu\.ppy\.sh\/(b|beatmapsets|beatmaps|s)\/(\d+)",
        RegexOptions.Compiled | RegexOptions.ECMAScript | RegexOptions.CultureInvariant
        );

        private static readonly Regex BeatmapsetRegex = new(
            @"http(?:s)*:\/\/osu\.ppy\.sh\/(beatmapsets|s)\/(\d+)",
            RegexOptions.Compiled | RegexOptions.ECMAScript | RegexOptions.CultureInvariant
        );

        private static readonly Regex BeatmapRegex = new(
            @"http(?:s)*:\/\/osu\.ppy\.sh\/(beatmaps|b)\/(\d+)",
            RegexOptions.Compiled | RegexOptions.ECMAScript | RegexOptions.CultureInvariant
        );

        private static readonly Regex BeatmapAndBeatmapsetRegex = new(
            @"http(?:s)*:\/\/osu\.ppy\.sh\/beatmapsets\/(\d+)#(osu|taiko|fruits|mania)\/(\d+)",
            RegexOptions.Compiled | RegexOptions.ECMAScript | RegexOptions.CultureInvariant
        );

        public static bool CheckMapUrl(string url, out string? mode, out int? id, out int? setId)
        {
            mode = default;
            id = default;
            setId = default;
        
            var match = BeatmapOrBeatmapsetRegex.Match(url);
            if (!match.Success) return false;

            var setMatch = BeatmapsetRegex.Match(url);
            if (setMatch.Success) setId = Convert.ToInt32(setMatch.Groups!.Values.Skip(2).First().Value);

            var idMatch = BeatmapRegex.Match(url);
            if (idMatch.Success) id = Convert.ToInt32(idMatch.Groups!.Values.Skip(2).First().Value);

            var fullMatch = BeatmapAndBeatmapsetRegex.Match(url);
            if (fullMatch.Success)
            {
                var groups = fullMatch.Groups.Values.ToList();
                id = Convert.ToInt32(groups[3].Value);
                mode = groups[2].Value;
                setId = Convert.ToInt32(groups[1].Value);
            }

            return setMatch.Success || idMatch.Success || fullMatch.Success;
        }
    }
}