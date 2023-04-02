using Disqord;
using Disqord.Bot.Commands;
using Humanizer;
using Pepper.Commons.Maimai;
using Pepper.Commons.Maimai.Structures.Data;
using Pepper.Frontends.Maimai.Commands.Button;
using Pepper.Frontends.Maimai.Database.MaimaiDxNetCookieProviders;
using Pepper.Frontends.Maimai.Services;
using Qmmands;
using Qmmands.Text;

namespace Pepper.Frontends.Maimai.Commands.Text
{
    public class User : MaimaiTextCommand
    {
        public User(MaimaiDxNetClientFactory factory, MaimaiDataService data, IMaimaiDxNetCookieProvider cookieProvider)
            : base(factory, data, cookieProvider) { }

        [TextCommand("maiuser", "user", "u")]
        [Description("Show info of an user.")]
        public async Task<IDiscordCommandResult> Exec(
            [Description("User in question")] IMember? player = null
        )
        {
            var cookie = await CookieProvider.GetCookie(player?.Id ?? Context.AuthorId);
            var client = ClientFactory.Create(cookie!);
            var user = await client.GetUserPlayData();
            var stats = user.UserStatistics;
            var isShinDan = user.DanLevel > 11;

            return Reply(new LocalEmbed
            {
                Title = $"{user.Name}",
                ThumbnailUrl = user.Avatar,
                Author = new LocalEmbedAuthor
                {
                    Name = $"{(isShinDan ? "Shin " : "")}{(user.DanLevel % 11).Ordinalize()} Dan - class {user.SeasonClass}"
                },
                Description = $"**{((player?.Id ?? Context.AuthorId) == 605384538113179658 ? user.Rating + 2000 : user.Rating)}** rating, cleared **{stats.Clear}** charts through **{user.PlayCount}** plays.",
                Fields = new List<LocalEmbedField>
                {
                    new ()
                    {
                        Name = "AP/FC",
                        Value = $"AP : **{stats.AllPerfect}** - **{stats.AllPerfectPlus}** (+)"
                                + "\n"
                                + $"FC : **{stats.FullCombo}** - **{stats.FullComboPlus}** (+)",
                        IsInline = true
                    },
                    new ()
                    {
                        Name = "Full Sync",
                        Value = $"**{stats.FullSync}** - **{stats.FullSyncPlus}** (+)"
                                + "\n"
                                + $"**{stats.FullSyncDx}** (DX) - **{stats.FullSyncDxPlus}** (DX+)",
                        IsInline = true
                    },
                    new () { Name = "\u200B", Value = "\u200B", IsInline = false },
                    new ()
                    {
                        Name = "Rankings",
                        Value = $"SSS : **{stats.SSS}** - {stats.SSSPlus} (+)"
                                + $"\nSS : **{stats.SS}** - {stats.SSPlus} (+)"
                                + $"\nS : **{stats.S}** - {stats.SPlus} (+)",
                        IsInline = true
                    },
                    new ()
                    {
                        Name = "DX scores (1/2/3/4/5)",
                        Value = $"**{stats.DxStar1}** / **{stats.DxStar2}** / **{stats.DxStar3}**" +
                                $" / **{stats.DxStar4}** / **{stats.DxStar5}**",
                        IsInline = true
                    }
                }
            });
        }

    }
}