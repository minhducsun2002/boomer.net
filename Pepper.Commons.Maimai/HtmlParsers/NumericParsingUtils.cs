using System.Runtime.CompilerServices;

namespace Pepper.Commons.Maimai.HtmlParsers
{
    public static class NumericParsingUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int FastIntParse(ReadOnlySpan<char> s)
        {
            var output = 0;
            var len = s.Length;
            for (var i = 0; i < len; i++)
            {
                var c = s[i];
                output = output * 10 + (c - '0');
            }

            return output;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int FastIntParseIgnoreCommaAndSpace(ReadOnlySpan<char> s)
        {
            var output = 0;
            var len = s.Length;
            for (var i = 0; i < len; i++)
            {
                var c = s[i];
                if (c is ',' or ' ')
                {
                    continue;
                }
                output = output * 10 + (c - '0');
            }

            return output;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static (int, int) ParseSlashedVsMaxStats(ReadOnlySpan<char> raw)
        {
            // format : [A,]BCD / [A,]BCD
            var len = raw.Length;
            var index = -1;
            for (var i = 0; i < len; i++)
            {
                if (raw[i] == '/')
                {
                    index = i;
                }
            }

            if (index == -1)
            {
                return (0, 0);
            }

            return (
                FastIntParseIgnoreCommaAndSpace(raw[..index]),
                FastIntParseIgnoreCommaAndSpace(raw[(index + 1)..])
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int ParseAccuracy(ReadOnlySpan<char> raw)
        {
            // form: A[B][C].EFGH%
            // 7 -> 1, 8 -> 2, 9 -> 3
            var markerIndex = raw.Length - 6;
            var p1 = raw[..markerIndex];
            var p2 = raw[(markerIndex + 1)..^1];
            return FastIntParse(p1) * 10000 + FastIntParse(p2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static (int, bool) ParseLevel(ReadOnlySpan<char> raw) =>
            raw[^1] == '+'
                ? (FastIntParse(raw[..^1]), true)
                : (FastIntParse(raw), false);
    }
}