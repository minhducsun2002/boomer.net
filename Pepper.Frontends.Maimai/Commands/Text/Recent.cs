using Disqord;
using Disqord.Bot.Commands;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Pepper.Commons.Maimai;
using Pepper.Commons.Maimai.Entities;
using Pepper.Commons.Maimai.Structures.Data;
using Pepper.Commons.Maimai.Structures.Data.Score;
using Pepper.Frontends.Maimai.Database.MaimaiDxNetCookieProviders;
using Pepper.Frontends.Maimai.Services;
using Pepper.Frontends.Maimai.Structures;
using Qmmands;
using Qmmands.Text;
using Difficulty = Pepper.Commons.Maimai.Entities.Difficulty;

namespace Pepper.Frontends.Maimai.Commands.Text
{
    public class Recent : MaimaiTextCommand
    {
        public Recent(HttpClient http, MaimaiDataService data, IMaimaiDxNetCookieProvider cookieProvider)
            : base(http, data, cookieProvider) { }

        [TextCommand("mairecent", "recent", "rs")]
        [Description("Show recent plays of an user.")]
        public async Task<IDiscordCommandResult> Exec(
            [Description("User in question")] IMember? player = null
        )
        {
            var cookie = await CookieProvider.GetCookie(player?.Id ?? Context.AuthorId);
            var client = new MaimaiDxNetClient(HttpClient, cookie!);
            var recent = await client.GetUserRecentRecord();
            var rs = recent.Select(record =>
            {
                if (record == null)
                {
                    return ((RecentRecord?) null, ((Difficulty, Song, bool)?) null);
                }

                var res = GameDataService.ResolveSongLoosely(record.Name, record.Difficulty, record.Version);
                if (res != null)
                {
                    var hasMultipleVersion = GameDataService.HasMultipleVersions(record.Name);
                    return (record, (res.Value.Item1, res.Value.Item2, hasMultipleVersion));
                }
                return (record, null);
            });

            return View(RecentScorePagedView.Create(rs), TimeSpan.FromSeconds(30));
        }
    }
}