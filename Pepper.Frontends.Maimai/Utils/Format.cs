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
                Difficulty.Basic => new Color(0x45c124),
                Difficulty.Advanced => new Color(0xffba01),
                Difficulty.Expert => new Color(0xff7b7b),
                Difficulty.Master => new Color(0x9f51dc),
                Difficulty.ReMaster => new Color(0xdbaaff),
                _ => new Color(0x45c124)
            };
            return color;
        }
    }
}