using System.Net.Http;
using Pepper.Commons.Maimai.Structures;
using Pepper.Database.MaimaiDxNetCookieProviders;
using Pepper.Services.Maimai;
using Pepper.Structures;
using Pepper.Structures.CommandAttributes.Metadata;

namespace Pepper.Commands.Maimai
{
    [Category("maimai")]
    public abstract class MaimaiCommand : Command
    {
        // TODO: Split game data DB requirement into another subclass. User needs no game data.
        protected readonly HttpClient HttpClient;
        protected readonly IMaimaiDxNetCookieProvider CookieProvider;
        protected readonly MaimaiDataService GameDataService;
        public MaimaiCommand(HttpClient httpClient, MaimaiDataService dataService, IMaimaiDxNetCookieProvider cookieProvider)
        {
            HttpClient = httpClient;
            CookieProvider = cookieProvider;
            GameDataService = dataService;
        }

        /// <param name="accuracy">Accuracy, in range [0, 1M]</param>
        /// <param name="chartConstant">Chart constant, in range [10, 150]</param>
        /// <returns></returns>
        protected static long GetFinalScore(long accuracy, long chartConstant)
        {
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
                if (accuracy > Coeff[i].Item1)
                {
                    return Coeff[i].Item2;
                }
            }

            return 0;
        }

        protected static string GetStatusString(FcStatus fcStatus)
        {
            var comboText = fcStatus switch
            {
                FcStatus.FC => "FC",
                FcStatus.FCPlus => "FC+",
                FcStatus.AllPerfect => "AP",
                FcStatus.AllPerfectPlus => "AP+",
                _ => ""
            };
            return comboText;
        }

        protected static string GetStatusString(SyncStatus syncStatus)
        {
            var syncText = syncStatus switch
            {
                SyncStatus.FullSyncDx => "FS DX",
                SyncStatus.FullSyncDxPlus => "FS DX+",
                SyncStatus.FullSync => "FS",
                SyncStatus.FullSyncPlus => "FS+",
                _ => ""
            };
            return syncText;
        }
    }
}