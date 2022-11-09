using System.Runtime.CompilerServices;
using Pepper.Commons.Maimai.Structures;

namespace Pepper.Commons.Maimai.HtmlParsers
{
    public partial class RecentRecordParser
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static DateTimeOffset ParseTime(ReadOnlySpan<char> raw)
        {
            // form: YYYY/MM/DD hh:mm
            var year = int.Parse(raw[..4]);
            var month = int.Parse(raw[5..7]);
            var date = int.Parse(raw[8..10]);
            var hour = int.Parse(raw[8..10]);
            var minute = int.Parse(raw[14..]);
            return new DateTimeOffset(year, month, date, hour, minute, 00, TimeSpan.FromHours(9));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int ParseAccuracy(ReadOnlySpan<char> raw)
        {
            // form: AB.CDEF%
            var markerIndex = raw.Length == 9 ? 3 : 2;
            var p1 = raw[..markerIndex];
            var p2 = raw[(markerIndex + 1)..^1];
            return int.Parse(p1) * 10000 + int.Parse(p2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string ParseRank(ReadOnlySpan<char> raw)
        {
            var len = raw.Length;
            for (var i = 51; i < len; i++)
            {
                if (raw[i] == '.')
                {
                    return raw[51..i].ToString();
                }
            }
            return "";
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
        
    }
}