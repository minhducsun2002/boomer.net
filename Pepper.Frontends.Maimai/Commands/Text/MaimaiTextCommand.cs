using System.Runtime.CompilerServices;
using Disqord;
using Pepper.Commons.Maimai.Structures.Enums;
using Pepper.Commons.Structures;
using Pepper.Commons.Structures.CommandAttributes.Metadata;
using Pepper.Frontends.Maimai.Database.MaimaiDxNetCookieProviders;
using Pepper.Frontends.Maimai.Services;

namespace Pepper.Frontends.Maimai.Commands.Text
{
    [Category("maimai")]
    public abstract class MaimaiTextCommand : Command
    {
        // TODO: Split game data DB requirement into another subclass. User needs no game data.
        protected readonly HttpClient HttpClient;
        protected readonly IMaimaiDxNetCookieProvider CookieProvider;
        protected readonly MaimaiDataService GameDataService;
        public MaimaiTextCommand(HttpClient httpClient, MaimaiDataService dataService, IMaimaiDxNetCookieProvider cookieProvider)
        {
            HttpClient = httpClient;
            CookieProvider = cookieProvider;
            GameDataService = dataService;
        }

        // Universe PLUS
        protected int LatestVersion => GameDataService.NewestVersion == 0 ? 18 : GameDataService.NewestVersion;
        protected static readonly Difficulty[] Difficulties =
        {
            Difficulty.Basic, Difficulty.Advanced, Difficulty.Expert, Difficulty.Master, Difficulty.ReMaster
        };

        /// <param name="accuracy">Accuracy, in range [0, 1005000]</param>
        /// <param name="chartConstant">Chart constant, in range [10, 150]</param>
        /// <returns></returns>
        public static long GetFinalScore(long accuracy, long chartConstant)
        {
            if (accuracy > 1005000)
            {
                accuracy = 1005000;
            }
            return accuracy * chartConstant * GetRankCoeff(accuracy);
        }

        private static readonly (int, int)[] Coeff = {
            (1005000, 224),
            (1000000, 216),
            (0995000, 211),
            (0990000, 208),
            (0980000, 203),
            (0970000, 200),
            (0940000, 168),
            (0900000, 136),
            (0800000, 080),
            (0750000, 075),
            (0700000, 070),
            (0600000, 060),
            (0500000, 050),
            (0400000, 040),
            (0300000, 030),
            (0200000, 020),
            (0100000, 010)
        };

        private static int GetRankCoeff(long accuracy)
        {
            for (var i = 0; i < Coeff.Length; i++)
            {
                if (accuracy >= Coeff[i].Item1)
                {
                    return Coeff[i].Item2;
                }
            }

            return 0;
        }

        public static string GetFormattedStatusString(FcStatus fcStatus)
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

        public static string GetFormattedStatusString(SyncStatus syncStatus)
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

        public static Color GetColor(Difficulty difficulty)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int NormalizeRating(long total)
        {
            return (int) (total / 1000000 / 10 / 10);
        }

        protected static readonly LocalEmoji Hourglass = new("⏳");
        protected static readonly LocalEmoji Failed = new("❌");
        protected static readonly LocalEmoji Success = new("✅");
    }
}