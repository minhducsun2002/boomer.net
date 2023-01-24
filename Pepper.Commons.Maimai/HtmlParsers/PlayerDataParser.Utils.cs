using System.Runtime.CompilerServices;
using Pepper.Commons.Maimai.Structures.Data;

namespace Pepper.Commons.Maimai.HtmlParsers
{
    internal static partial class PlayerDataParser
    {
        private static UserStatistics GetUserStatistics(Dictionary<string, string> stats)
        {
            var app = SplitStatRecord(stats["app"]);
            var ap = SplitStatRecord(stats["ap"]);
            var fcp = SplitStatRecord(stats["fcp"]);
            var fc = SplitStatRecord(stats["fc"]);
            var fsd = SplitStatRecord(stats["fsd"]);
            var fsdp = SplitStatRecord(stats["fsdp"]);
            var fsp = SplitStatRecord(stats["fsp"]);
            var fs = SplitStatRecord(stats["fs"]);

            var sssp = SplitStatRecord(stats["sssp"]);
            var sss = SplitStatRecord(stats["sss"]);
            var ssp = SplitStatRecord(stats["ssp"]);
            var ss = SplitStatRecord(stats["ss"]);
            var sp = SplitStatRecord(stats["sp"]);
            var s = SplitStatRecord(stats["s"]);

            var clear = SplitStatRecord(stats["clear"]);

            var dx5 = SplitStatRecord(stats["dxstar_5"]);
            var dx4 = SplitStatRecord(stats["dxstar_4"]);
            var dx3 = SplitStatRecord(stats["dxstar_3"]);
            var dx2 = SplitStatRecord(stats["dxstar_2"]);
            var dx1 = SplitStatRecord(stats["dxstar_1"]);
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
                    return FastIntParseIgnoreCommaAndSpace(s[..i]);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        internal static int FastIntParse(ReadOnlySpan<char> s)
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
        internal static int FastIntParseIgnoreCommaAndSpace(ReadOnlySpan<char> s)
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
    }
}