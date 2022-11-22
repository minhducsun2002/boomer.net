using System.Runtime.CompilerServices;
using Pepper.Commons.Maimai.Structures;

namespace Pepper.Commons.Maimai.HtmlParsers
{
    public partial class AllRecordParser
    {
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        internal static (int, int) ParseSlashedVsMaxStats(ReadOnlySpan<char> raw)
        {
            // format : [A,]BCD / [A,]BCD
            int f1 = 0, f2 = 0, len = raw.Length;
            var blown = false;
            for (var i = 0; i < len; i++)
            {
                if (raw[i] == ',' || raw[i] == ' ')
                {
                    continue;
                }

                if (raw[i] == '/')
                {
                    blown = true;
                    continue;
                }

                if (blown)
                {
                    f2 = f2 == 0
                        ? raw[i] - '0'
                        : 10 * f2 + (raw[i] - '0');
                }
                else
                {
                    f1 = f1 == 0
                        ? raw[i] - '0'
                        : 10 * f1 + (raw[i] - '0');
                }
            }

            return (f1, f2);
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
        private static string ParseRank(ReadOnlySpan<char> raw)
        {
            var len = raw.Length;
            for (var i = 54; i < len; i++)
            {
                if (raw[i] == '.')
                {
                    return raw[54..i].ToString();
                }
            }
            return "";
        }
    }
}