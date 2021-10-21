using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pepper.Utilities.Osu
{
    public static partial class URLParser
    {
        private static readonly Regex ScoreUrlRegex = new(
            @"http(?:s)*:\/\/osu\.ppy\.sh\/scores\/(osu|taiko|fruits|mania)\/(\d+)",
                RegexOptions.Compiled | RegexOptions.ECMAScript | RegexOptions.CultureInvariant
        );

        public static bool CheckScoreUrl(string url, out (string, long) @out)
        {
            var match = ScoreUrlRegex.Match(url);
            @out = default;
            if (!match.Success)
            {
                return false;
            }

            var _ = match.Groups!.Values!.Skip(1).Take(2).Select(group => group.Value).ToArray();
            @out = (_[0], Convert.ToInt64(_[^1]));
            return true;
        }
    }
}