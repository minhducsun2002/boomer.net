using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Humanizer;
using Pepper.Commons.Maimai;
using Pepper.Database.MaimaiDxNetCookieProviders;
using Qmmands;
using Qmmands.Text;

namespace Pepper.Commands.Maimai
{
    public class User : MaimaiCommand
    {
        public User(HttpClient httpClient, MaimaiDbContext dbContext, IMaimaiDxNetCookieProvider cookieProvider) 
            : base(httpClient, dbContext, cookieProvider) {}

        [TextCommand("maiuser")]
        [Description("Show info of an user.")]
        public async Task<IDiscordCommandResult> Exec(
            [Description("User in question")] IMember? player = null
        )
        {
            var cookie = await CookieProvider.GetCookie(player?.Id ?? Context.AuthorId);
            var client = new MaimaiDxNetClient(HttpClient, cookie!);
            var user = await client.GetUserPlayData();
            var stats = user.UserStatistics;

            return Reply(new LocalEmbed
            {
                Title = $"{user.Name}",
                ThumbnailUrl = user.Avatar,
                Author = new LocalEmbedAuthor
                {
                    Name = $"Dan {user.DanLevel.Ordinalize()} - class {user.SeasonClass}"
                },
                Description = $"**{user.Rating}** rating, cleared **{stats.Clear}** charts through **{user.PlayCount}** plays.",
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