using System.Runtime.CompilerServices;
using Disqord;
using Pepper.Commons.Maimai.Structures.Data.Enums;
using Pepper.Commons.Structures;
using Pepper.Commons.Structures.CommandAttributes.Metadata;
using Pepper.Frontends.Maimai.Database.MaimaiDxNetCookieProviders;
using Pepper.Frontends.Maimai.Services;
using Pepper.Frontends.Maimai.Utils;

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

        protected static readonly LocalEmoji Hourglass = new("⏳");
        protected static readonly LocalEmoji Failed = new("❌");
        protected static readonly LocalEmoji Success = new("✅");
    }
}