using Disqord;
using Pepper.Commons.Maimai.Structures.Enums;

namespace Pepper.Frontends.Maimai.Utils
{
    public static class Format
    {
        public static string Status(FcStatus fcStatus)
        {
            var comboText = fcStatus switch
            {
                FcStatus.FC => "**FC**",
                FcStatus.FCPlus => "**FC**+",
                FcStatus.AllPerfect => "**AP**",
                FcStatus.AllPerfectPlus => "**AP**+",
                _ => ""
            };
            return comboText;
        }

        public static string Status(SyncStatus syncStatus)
        {
            var syncText = syncStatus switch
            {
                SyncStatus.FullSyncDx => "**FS DX**",
                SyncStatus.FullSyncDxPlus => "**FS DX**+",
                SyncStatus.FullSync => "**FS**",
                SyncStatus.FullSyncPlus => "**FS**+",
                _ => ""
            };
            return syncText;
        }

        public static Color Color(Difficulty difficulty)
        {
            var color = difficulty switch
            {
                Difficulty.Basic => new Color(0x6fe163),
                Difficulty.Advanced => new Color(0xf8df3a),
                Difficulty.Expert => new Color(0xff828e),
                Difficulty.Master => new Color(0xc27ff4),
                Difficulty.ReMaster => new Color(0xe5ddea),
                _ => new Color(0x6fe163)
            };
            return color;
        }
    }
}