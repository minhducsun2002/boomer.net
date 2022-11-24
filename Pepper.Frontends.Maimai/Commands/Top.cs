using Disqord;
using Disqord.Bot.Commands;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Rest;
using Pepper.Commons.Maimai;
using Pepper.Commons.Maimai.Entities;
using Pepper.Commons.Maimai.Structures;
using Pepper.Frontends.Maimai.Database.MaimaiDxNetCookieProviders;
using Pepper.Frontends.Maimai.Services;
using Qmmands;
using Qmmands.Text;
using PagedView = Pepper.Commons.Structures.Views.PagedView;

namespace Pepper.Frontends.Maimai.Commands
{
    public class Top : TopScoreCommand
    {
        public Top(HttpClient http, MaimaiDataService data, IMaimaiDxNetCookieProvider cookieProvider) : base(http, data, cookieProvider) { }

        private static readonly string[] DifficultyStrings =
        {
            "BASIC", "ADVANCED", "EXPERT", "MASTER", "Re:MASTER"
        };

        [TextCommand("maitop", "top")]
        [Description("Show top rated plays of an user.")]
        public async Task<IDiscordCommandResult> Exec(
            [Description("User in question")] IMember? player = null
        )
        {
            var cookie = await CookieProvider.GetCookie(player?.Id ?? Context.AuthorId);
            var client = new MaimaiDxNetClient(HttpClient, cookie!);

            var msg = await Reply($"A few seconds please, loading {Difficulties.Length} pages...");

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
                await msg.DeleteAsync();
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
                .WithText($"Total New rating : {NormalizeRating(newScores.Select(s => s.total).Sum())}");
            var oldFooter = new LocalEmbedFooter()
                .WithText($"Total Old rating : {NormalizeRating(oldScores.Select(s => s.total).Sum())}");

            var newEmbed = newScores
                .Chunk(3)
                .Select((entries, i1) =>
                {
                    var page = entries.Select((e, i2) =>
                    {
                        var (score, diff, song, total, version) = e;
                        var embed = CreateEmbed(score, diff, song, total, i1 * 3 + i2);
                        embed = embed.WithFooter(newFooter);
                        return embed;
                    });
                    return page;
                });

            var oldEmbed = oldScores
                .Chunk(3)
                .Select((entries, i1) =>
                {
                    var page = entries.Select((e, i2) =>
                    {
                        var (score, diff, song, total, version) = e;
                        var embed = CreateEmbed(score, diff, song, total, i1 * 3 + i2);
                        embed = embed.WithFooter(oldFooter);
                        return embed;
                    });
                    return page;
                });

            var embeds = newEmbed
                .Concat(oldEmbed)
                .Select(embed => new Page().WithEmbeds(embed).WithContent("These calculations are estimated."))
                .ToArray();

            await msg.DeleteAsync();
            return View(new PagedView(new ListPageProvider(embeds)), TimeSpan.FromSeconds(30));
        }

        private LocalEmbed CreateEmbed(ScoreRecord score, Pepper.Commons.Maimai.Entities.Difficulty? diff, Song? song, long total, int index = 0)
        {
            var diffText = DifficultyStrings[(int) score.Difficulty];
            var accuracy = score.Accuracy;
            int chartConstant;
            if (diff != null)
            {
                chartConstant = diff.Level * 10 + diff.LevelDecimal;
            }
            else
            {
                var d = score.Level;
                chartConstant = d.Item1 * 10 + (d.Item2 ? 7 : 0);
            }

            var rankEndingInPlus = score.Rank[^1] == 'p';
            var hasMultipleVersions = GameDataService.HasMultipleVersions(score.Name);

            var embed = new LocalEmbed
            {
                Author = new LocalEmbedAuthor()
                    .WithName($"{index + 1}. "
                              + score.Name
                              + (score.Version == ChartVersion.Deluxe && hasMultipleVersions ? "  [DX] " : "  ")
                              + $"[{diffText} {chartConstant / 10}.{chartConstant % 10}]"),
                Description = $"**{accuracy / 10000}**.**{accuracy % 10000:0000}**%"
                              + " - ["
                              + (rankEndingInPlus
                                  ? $"**{score.Rank[..^1].ToUpperInvariant()}**+"
                                  : $"**{score.Rank.ToUpperInvariant()}**")
                              + $"] - **{NormalizeRating(total)}** rating",
                Color = GetColor(score.Difficulty)
            };
            var imageUrl = GameDataService.GetImageUrl(score.Name);
            if (imageUrl != null)
            {
                embed = embed.WithThumbnailUrl(imageUrl);
            }

            if (song != null)
            {
                embed.AddField("Genre", song.Genre!.Name, true);
                embed.AddField("Version", song.AddVersion!.Name, true);
                embed.AddField("Level", $"{score.Level.Item1}{(score.Level.Item2 ? "+" : "")}", true);
            }
            return embed;
        }
    }
}