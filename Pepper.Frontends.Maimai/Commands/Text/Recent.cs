using Disqord;
using Disqord.Bot.Commands;
using Pepper.Commons.Maimai;
using Pepper.Commons.Maimai.Structures.Data.Score;
using Pepper.Frontends.Maimai.Database.MaimaiDxNetCookieProviders;
using Pepper.Frontends.Maimai.Services;
using Pepper.Frontends.Maimai.Structures;
using Qmmands;
using Qmmands.Text;

namespace Pepper.Frontends.Maimai.Commands.Text
{
    public class Recent : MaimaiTextCommand
    {
        public Recent(MaimaiDxNetClientFactory factory, MaimaiDataService data, IMaimaiDxNetCookieProvider cookieProvider)
            : base(factory, data, cookieProvider) { }

        [TextCommand("mairecent", "recent", "rs")]
        [Description("Show recent plays of an user.")]
        public async Task<IDiscordCommandResult> Exec(
            [Description("User in question")] IMember? player = null
        )
        {
            var cookie = await CookieProvider.GetCookie(player?.Id ?? Context.AuthorId);
            var client = ClientFactory.Create(cookie!);
            var recent = await client.GetUserRecentRecord();
            var rs = recent.Select(record =>
            {
                if (record == null)
                {
                    return null;
                }

                var res = GameDataService.ResolveSongLoosely(record.Name, record.Difficulty, record.Version);
                var diff = res?.Item1;
                var song = res?.Item2;

                return new ScoreWithMeta<RecentRecord>(
                    record, song, diff,
                    song?.AddVersionId ?? GameDataService.NewestVersion,
                    GameDataService.HasMultipleVersions(record.Name),
                    GameDataService.GetImageUrl(record.Name)
                );
            });

            return View(RecentScorePagedView.Create(rs), TimeSpan.FromSeconds(30));
        }
    }
}