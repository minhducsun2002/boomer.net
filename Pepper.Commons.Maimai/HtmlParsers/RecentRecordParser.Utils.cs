using System.Runtime.CompilerServices;
using Pepper.Commons.Maimai.Structures;
using Pepper.Commons.Maimai.Structures.Enums;

namespace Pepper.Commons.Maimai.HtmlParsers
{
    public partial class RecentRecordParser
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        internal static DateTimeOffset ParseTime(ReadOnlySpan<char> raw)
        {
            // form: YYYY/MM/DD hh:mm
            var year = PlayerDataParser.FastIntParse(raw[..4]);
            var month = PlayerDataParser.FastIntParse(raw[5..7]);
            var date = PlayerDataParser.FastIntParse(raw[8..10]);
            var hour = PlayerDataParser.FastIntParse(raw[11..13]);
            var minute = PlayerDataParser.FastIntParse(raw[14..]);
            return new DateTimeOffset(year, month, date, hour, minute, 00, TimeSpan.FromHours(9));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int ParseAccuracy(ReadOnlySpan<char> raw)
        {
            // form: A[B][C].EFGH%
            // 7 -> 1, 8 -> 2, 9 -> 3
            var markerIndex = raw.Length - 6;
            var p1 = raw[..markerIndex];
            var p2 = raw[(markerIndex + 1)..^1];
            return PlayerDataParser.FastIntParse(p1) * 10000 + PlayerDataParser.FastIntParse(p2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static (string, bool) ParseRank(ReadOnlySpan<char> raw)
        {
            // https://maimaidx-eng.com/maimai-mobile/img/playlog/ssplus.png?ver=1.25
            var len = raw.Length;
            for (var i = 51; i < len; i++)
            {
                if (raw[i] == '.')
                {
                    var isPlus = raw[i - 2] == 'u';
                    return (raw[51..(isPlus ? i - 4 : i)].ToString(), isPlus);
                }
            }
            return ("", false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ChartVersion ParseVersion(ReadOnlySpan<char> raw) =>
            // https://maimaidx-eng.com/maimai-mobile/img/music_(dx,standard).png
            raw[49] == 'd'
                ? ChartVersion.Deluxe
                : ChartVersion.Standard;

        private static FcStatus ParseFcStatus(ReadOnlySpan<char> raw)
        {
            // https://maimaidx-eng.com/maimai-mobile/img/playlog/(fc,ap)(plus?).png?ver=1.25
            return raw[51] switch
            {
                'f' => raw[53] switch
                {
                    'p' => FcStatus.FCPlus,
                    '.' => FcStatus.FC,
                    // '_' => FcStatus.None,
                    _ => FcStatus.None
                },
                'a' => raw[53] switch
                {
                    '.' => FcStatus.AllPerfect,
                    'p' => FcStatus.AllPerfectPlus,
                    _ => FcStatus.None
                },
                _ => FcStatus.None
            };
        }

        private static int ParseMultiplayerRank(ReadOnlySpan<char> raw) => raw[51] - '0';

        private static SyncStatus ParseSyncStatus(ReadOnlySpan<char> raw)
        {
            return raw[53] switch
            {
                'd' => raw[54] switch
                {
                    'p' => SyncStatus.FullSyncDxPlus,
                    '.' => SyncStatus.FullSyncDx,
                    _ => SyncStatus.FullSyncDx
                },
                'p' => SyncStatus.FullSyncPlus,
                '.' => SyncStatus.FullSync,
                '_' => SyncStatus.None,
                _ => SyncStatus.None
            };
        }

        internal static Difficulty ParseDifficulty(ReadOnlySpan<char> raw)
        {
            // https://maimaidx-eng.com/maimai-mobile/img/diff_expert.png
            return raw[48] switch
            {
                'b' => Difficulty.Basic,
                'a' => Difficulty.Advanced,
                'e' => Difficulty.Expert,
                'm' => Difficulty.Master,
                'r' => Difficulty.ReMaster,
                _ => Difficulty.None
            };
        }

        private static ChallengeType ParseChallengeType(ReadOnlySpan<char> raw)
        {
            // https://maimaidx-eng.com/maimai-mobile/img/icon_perfectchallenge.png
            // https://maimaidx-eng.com/maimai-mobile/img/course/icon_course.png
            return raw[43] switch
            {
                'c' => ChallengeType.Course,
                'i' => ChallengeType.PerfectChallenge,
                _ => ChallengeType.None
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static bool ParseWinningMatching(ReadOnlySpan<char> raw)
        {
            // https://maimaidx-eng.com/maimai-mobile/img/playlog/win.png;
            return raw[51] == 'w';
        }
    }
}