using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pepper.Utilities.Osu
{
    public static class URLParser
    {
        private static readonly Regex ScoreUrlRegex =
            new Regex(@"http(?:s)*:\/\/osu\.ppy\.sh\/scores\/(osu|taiko|fruits|mania)\/(\d+)",
                RegexOptions.Compiled | RegexOptions.ECMAScript | RegexOptions.CultureInvariant);
        
        public static bool CheckScoreUrl(string url, out (string, long) @out)
        {
            var match = ScoreUrlRegex.Matches(url);
            @out = default;
            if (match.Count == 0) return false;

            var _ = match[0].Groups!.Values!.Skip(1).Take(2).Select(group => group.Value).ToArray();
            @out = (_[0], Convert.ToInt64(_[^1]));
            return true;
        }
    }
}