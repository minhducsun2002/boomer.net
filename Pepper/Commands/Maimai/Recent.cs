using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Pepper.Commons.Maimai;
using Pepper.Commons.Maimai.Structures;
using Pepper.Database.MaimaiDxNetCookieProviders;
using Qmmands;
using Qmmands.Text;
using Qommon;
using PagedView = Pepper.Structures.PagedView;

namespace Pepper.Commands.Maimai
{
    public class Recent : MaimaiCommand
    {
        public Recent(HttpClient http, MaimaiDbContext db, IMaimaiDxNetCookieProvider cookieProvider)
            : base(http, db, cookieProvider) {}
        
        [TextCommand("mairecent")]
        [Description("Show recent plays of an user.")]
        public async Task<IDiscordCommandResult> Exec(
            [Description("User in question")] IMember? player = null
        )
        {
            var cookie = await CookieProvider.GetCookie(player?.Id ?? Context.AuthorId);
            var client = new MaimaiDxNetClient(HttpClient, cookie!);
            var recent = await client.GetUserRecentRecord();

            var embeds = recent
                .Chunk(4)
                .Select(recordGroup =>
                {
                    var embeds = recordGroup.Select(record =>
                    {
                        var diff = record.Difficulty == Difficulty.ReMaster
                            ? "Re:MASTER"
                            : record.Difficulty.ToString().ToUpperInvariant();
                        var color = record.Difficulty switch
                        {
                            Difficulty.Basic => new Color(0x45c124),
                            Difficulty.Advanced => new Color(0xffba01),
                            Difficulty.Expert => new Color(0xff7b7b),
                            Difficulty.Master => new Color(0x9f51dc),
                            Difficulty.ReMaster => new Color(0xdbaaff),
                            _ => new Color(0x45c124)
                        };
                        var rankEndingInPlus = record.Rank.EndsWith("plus");
                        var comboText = record.FcStatus switch
                        {
                            FcStatus.FC => "FC",
                            FcStatus.FCPlus => "FC+",
                            FcStatus.AllPerfect => "AP",
                            FcStatus.AllPerfectPlus => "AP+",
                            _ => ""
                        };

                        var syncText = record.SyncStatus switch
                        {
                            SyncStatus.FullSyncDx => "FS DX",
                            SyncStatus.FullSyncDxPlus => "FS DX+",
                            SyncStatus.FullSync => "FS",
                            SyncStatus.FullSyncPlus => "FS+",
                            _ => ""
                        };
                        
                        var r = new LocalEmbed
                        {
                            Author = new LocalEmbedAuthor().WithName($"Track {record.Track} - {diff}"),
                            Title = $"{record.Name}",
                            Description = $"**{record.Accuracy / 10000}**.**{record.Accuracy % 10000}**%" +
                                          $" - **{(rankEndingInPlus ? record.Rank[..^4].ToUpperInvariant() : record.Rank.ToUpperInvariant())}**"
                                          + (rankEndingInPlus ? "+" : "")
                                          + (comboText == "" ? comboText : $" [**{comboText}**]")
                                          + (syncText == "" ? syncText : $" [**{syncText}**]"),
                            ThumbnailUrl = record.ImageUrl ?? Optional<string>.Empty,
                            Timestamp = record.Timestamp,
                            Color = color
                        };
                        return r;
                    });
                    return new Page().WithEmbeds(embeds);
                });

            return View(new PagedView(new ListPageProvider(embeds)), TimeSpan.FromSeconds(30));
        }
    }
}