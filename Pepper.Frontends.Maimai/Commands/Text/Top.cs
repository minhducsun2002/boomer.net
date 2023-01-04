using Disqord;
using Disqord.Bot.Commands;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Rest;
using Pepper.Commons.Maimai;
using Pepper.Commons.Maimai.Entities;
using Pepper.Commons.Maimai.Structures;
using Pepper.Commons.Maimai.Structures.Enums;
using Pepper.Commons.Maimai.Structures.Score;
using Pepper.Frontends.Maimai.Database.MaimaiDxNetCookieProviders;
using Pepper.Frontends.Maimai.Services;
using Pepper.Frontends.Maimai.Structures;
using Qmmands;
using Qmmands.Text;
using PagedView = Pepper.Commons.Structures.Views.PagedView;

namespace Pepper.Frontends.Maimai.Commands.Text
{
    public class Top : TopScoreCommand
    {
        public Top(HttpClient http, MaimaiDataService data, IMaimaiDxNetCookieProvider cookieProvider) : base(http, data, cookieProvider) { }

        [TextCommand("maitop", "top")]
        [Description("Show top rated plays of an user.")]
        public async Task<IDiscordCommandResult> Exec(
            [Description("User in question")] IMember? player = null
        )
        {
            var cookie = await CookieProvider.GetCookie(player?.Id ?? Context.AuthorId);
            var client = new MaimaiDxNetClient(HttpClient, cookie!);

            var records = await ListAllScores(client);

            var scores = records
                .AsParallel()
                .WithDegreeOfParallelism(8)
                .Select(score =>
                {
                    int chartConstant;
                    var searchRes = GameDataService.ResolveSongExact(score.Name, score.Difficulty, score.Level);
                    var version = GameDataService.NewestVersion;
                    if (searchRes.HasValue)
                    {
                        var (diff, song) = searchRes.Value;
                        version = song.AddVersionId;
                        chartConstant = diff.Level * 10 + diff.LevelDecimal;
                    }
                    else
                    {
                        var diff = score.Level;
                        chartConstant = diff.Item1 * 10 + (diff.Item2 ? 7 : 0);
                    }

                    var total = GetFinalScore(score.Accuracy, chartConstant);

                    if (searchRes.HasValue)
                    {
                        var (diff, song) = searchRes.Value;
                        return (score, diff, song, total, version);
                    }
#pragma warning disable CS8619
                    return (score, null, null, total, version);
#pragma warning restore CS8619
                })
                .OrderByDescending(p => p.total)
                .ToList();

            if (scores.Count == 0)
            {
                return Reply("No score was found!");
            }

            var newScores = scores
                .Where(s => s.version == LatestVersion)
                .Take(15)
                .ToList();
            var oldScores = scores
                .Where(s => s.version != LatestVersion)
                .Take(35)
                .ToList();
            var newFooter = new LocalEmbedFooter()
                .WithText($"Total New rating : {newScores.Select(s => NormalizeRating(s.total)).Sum()}");
            var oldFooter = new LocalEmbedFooter()
                .WithText($"Total Old rating : {oldScores.Select(s => NormalizeRating(s.total)).Sum()}");

            var newEmbed = newScores
                .Chunk(3)
                .Select((entries, i1) =>
                {
                    var page = entries.Select((e, i2) =>
                    {
                        var (score, diff, song, total, version) = e;
                        var embed = CreateEmbed(score, diff, song, total, i1 * 3 + i2);
                        return embed;
                    })
                        .Append(new LocalEmbed().WithFooter(newFooter));
                    return page;
                })
                .ToArray();

            var oldEmbed = oldScores
                .Chunk(3)
                .Select((entries, i1) =>
                {
                    var page = entries.Select((e, i2) =>
                    {
                        var (score, diff, song, total, version) = e;
                        var embed = CreateEmbed(score, diff, song, total, i1 * 3 + i2);
                        return embed;
                    })
                        .Append(new LocalEmbed().WithFooter(oldFooter)); ;
                    return page;
                });

            var embeds = newEmbed
                .Concat(oldEmbed)
                .Select(embed => new Page().WithEmbeds(embed).WithContent("These calculations are estimated."))
                .ToArray();

            return View(new TopScorePagedView(new ListPageProvider(embeds), newEmbed.Length), TimeSpan.FromSeconds(30));
        }

        private LocalEmbed CreateEmbed(TopRecord score, Pepper.Commons.Maimai.Entities.Difficulty? diff, Song? song, long total, int index = 0)
        {
            var hasMultipleVersions = GameDataService.HasMultipleVersions(score.Name);
            var imageUrl = GameDataService.GetImageUrl(score.Name);
            return ScoreFormatter.FormatScore(score, diff, song, index + 1, imageUrl, score.Level, hasMultipleVersions);
        }
    }
}