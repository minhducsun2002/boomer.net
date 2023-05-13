using System.Runtime.CompilerServices;
using Pepper.Commons.Maimai.Structures.Data.Enums;

namespace Pepper.Commons.Maimai.HtmlParsers
{
    public static class ImageLinkParsingUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static FcStatus ParseFcStatus(ReadOnlySpan<char> raw)
        {
            // https://maimaidx-eng.com/maimai-mobile/img/music_icon_fcp.png?ver=1.25
            return raw[54] switch
            {
                'f' => raw[56] switch
                {
                    'p' => FcStatus.FCPlus,
                    _ => FcStatus.FC
                },
                'a' => raw[56] switch
                {
                    'p' => FcStatus.AllPerfectPlus,
                    _ => FcStatus.AllPerfect
                },
                _ => FcStatus.None
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static SyncStatus ParseSyncStatus(ReadOnlySpan<char> raw)
        {
            // https://maimaidx-eng.com/maimai-mobile/img/music_icon_fsp.png?ver=1.25
            return raw[54] switch
            {
                'f' => raw[56] switch
                {
                    'd' => raw[57] switch
                    {
                        'p' => SyncStatus.FullSyncDxPlus,
                        _ => SyncStatus.FullSyncDx
                    },
                    'p' => SyncStatus.FullSyncPlus,
                    _ => SyncStatus.FullSync
                },
                _ => SyncStatus.None
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static (string, bool) ParseRank(ReadOnlySpan<char> raw)
        {
            // https://maimaidx-eng.com/maimai-mobile/img/music_icon_ssp.png?ver=1.25
            var len = raw.Length;
            for (var i = 54; i < len; i++)
            {
                if (raw[i] == '.')
                {
                    var isPlus = raw[i - 1] == 'p';
                    return (raw[54..(isPlus ? i - 1 : i)].ToString(), isPlus);
                }
            }
            return ("", false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static ChartVersion ParseVersion(ReadOnlySpan<char> raw) =>
            // https://maimaidx-eng.com/maimai-mobile/img/music_(dx,standard).png
            raw[49] == 'd'
                ? ChartVersion.Deluxe
                : ChartVersion.Standard;

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Difficulty ParseDifficulty(ReadOnlySpan<char> raw)
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
    }
}