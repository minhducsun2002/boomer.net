using System.Runtime.CompilerServices;
using Pepper.Commons.Maimai.Structures;
using Pepper.Commons.Maimai.Structures.Enums;

namespace Pepper.Commons.Maimai.HtmlParsers
{
    public partial class AllRecordParser
    {
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        internal static (int, int) ParseSlashedVsMaxStats(ReadOnlySpan<char> raw)
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
                PlayerDataParser.FastIntParseIgnoreCommaAndSpace(raw[..index]),
                PlayerDataParser.FastIntParseIgnoreCommaAndSpace(raw[(index + 1)..])
            );
        }

        private static FcStatus ParseFcStatus(ReadOnlySpan<char> raw)
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

        private static SyncStatus ParseSyncStatus(ReadOnlySpan<char> raw)
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

        private static (int, bool) ParseLevel(ReadOnlySpan<char> raw) =>
            raw[^1] == '+'
                ? (PlayerDataParser.FastIntParse(raw[..^1]), true)
                : (PlayerDataParser.FastIntParse(raw), false);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (string, bool) ParseRank(ReadOnlySpan<char> raw)
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
    }
}