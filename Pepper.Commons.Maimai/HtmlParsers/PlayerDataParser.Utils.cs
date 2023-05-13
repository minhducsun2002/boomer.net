using System.Runtime.CompilerServices;
using Pepper.Commons.Maimai.Structures.Data;

namespace Pepper.Commons.Maimai.HtmlParsers
{
    internal static partial class PlayerDataParser
    {
        private static UserStatistics GetUserStatistics(Dictionary<string, string> stats)
        {
            var app = SplitStatRecord((ReadOnlySpan<char>) stats["app"]);
            var ap = SplitStatRecord((ReadOnlySpan<char>) stats["ap"]);
            var fcp = SplitStatRecord((ReadOnlySpan<char>) stats["fcp"]);
            var fc = SplitStatRecord((ReadOnlySpan<char>) stats["fc"]);
            var fsd = SplitStatRecord((ReadOnlySpan<char>) stats["fsd"]);
            var fsdp = SplitStatRecord((ReadOnlySpan<char>) stats["fsdp"]);
            var fsp = SplitStatRecord((ReadOnlySpan<char>) stats["fsp"]);
            var fs = SplitStatRecord((ReadOnlySpan<char>) stats["fs"]);

            var sssp = SplitStatRecord((ReadOnlySpan<char>) stats["sssp"]);
            var sss = SplitStatRecord((ReadOnlySpan<char>) stats["sss"]);
            var ssp = SplitStatRecord((ReadOnlySpan<char>) stats["ssp"]);
            var ss = SplitStatRecord((ReadOnlySpan<char>) stats["ss"]);
            var sp = SplitStatRecord((ReadOnlySpan<char>) stats["sp"]);
            var s = SplitStatRecord((ReadOnlySpan<char>) stats["s"]);

            var clear = SplitStatRecord((ReadOnlySpan<char>) stats["clear"]);

            var dx5 = SplitStatRecord((ReadOnlySpan<char>) stats["dxstar_5"]);
            var dx4 = SplitStatRecord((ReadOnlySpan<char>) stats["dxstar_4"]);
            var dx3 = SplitStatRecord((ReadOnlySpan<char>) stats["dxstar_3"]);
            var dx2 = SplitStatRecord((ReadOnlySpan<char>) stats["dxstar_2"]);
            var dx1 = SplitStatRecord((ReadOnlySpan<char>) stats["dxstar_1"]);
            return new UserStatistics
            {
                AllPerfect = ap,
                AllPerfectPlus = app,
                FullCombo = fc,
                FullComboPlus = fcp,
                FullSyncDx = fsd,
                FullSyncDxPlus = fsdp,
                FullSync = fs,
                FullSyncPlus = fsp,

                Clear = clear,

                DxStar1 = dx1,
                DxStar2 = dx2,
                DxStar3 = dx3,
                DxStar4 = dx4,
                DxStar5 = dx5,

                SSS = sss,
                SSSPlus = sssp,
                SS = ss,
                SSPlus = ssp,
                S = s,
                SPlus = sp
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static int SplitStatRecord(ReadOnlySpan<char> s)
        {
            var len = s.Length;
            for (var i = 0; i < len; i++)
            {
                if (s[i] == '/')
                {
                    return NumericParsingUtils.FastIntParseIgnoreCommaAndSpace(s[..i]);
                }
            }

            return 0;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (int, int)? GetTwoDigitsData(string prefix, string data)
        {
            var index = data.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
            if (index == -1)
            {
                return null;
            }

            var len = prefix.Length;
            var c1 = data[index + len];
            var c2 = data[index + len + 1];
            return (c1 - '0', c2 - '0');
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ReadOnlySpan<char> GetIntFromString(ReadOnlySpan<char> s)
        {
            var len = s.Length;
            for (var i = 0; i < len; i++)
            {
                if (char.IsDigit(s[i]))
                {
                    return s[i..];
                }
            }

            return ReadOnlySpan<char>.Empty;
        }
    }
}