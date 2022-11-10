using System;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Rest;
using Pepper.Commons.Maimai;
using Pepper.Commons.Maimai.Structures;
using Pepper.Database.MaimaiDxNetCookieProviders;
using Pepper.Services.Maimai;
using Qmmands;
using Qmmands.Text;
using PagedView = Pepper.Structures.PagedView;

namespace Pepper.Commands.Maimai
{
    public class Top : MaimaiCommand
    {
        public Top(HttpClient http, MaimaiDataService data, IMaimaiDxNetCookieProvider cookieProvider) : base(http, data, cookieProvider) {}

        private readonly Difficulty[] difficulties =
        {
            Difficulty.Basic, Difficulty.Advanced, Difficulty.Expert, Difficulty.Master, Difficulty.ReMaster
        };
        
        [TextCommand("maitop")]
        [Description("Show top rated plays of an user.")]
        public async Task<IDiscordCommandResult> Exec(
            [Description("User in question")] IMember? player = null
        )
        {
            var cookie = await CookieProvider.GetCookie(player?.Id ?? Context.AuthorId);
            var client = new MaimaiDxNetClient(HttpClient, cookie!);
            
            var msg = await Reply($"A few seconds please, loading {difficulties.Length} pages...");
            
            // Universe PLUS
            var latestVersion = GameDataService.NewestVersion == 0 ? 18 : GameDataService.NewestVersion;
            var records = Enumerable.Empty<ScoreRecord>();
            foreach (var diff in difficulties)
            {
                records = records.Concat(await client.GetUserDifficultyRecord(diff));
            }

            var scores = records
                .AsParallel()
                .WithDegreeOfParallelism(8)
                .Select(score =>
                {
                    int chartConstant;
                    var searchRes = GameDataService.ResolveSong(score.Name, score.Difficulty, score.Level);
                    var version = GameDataService.NewestVersion;
                    var difficulty = score.Difficulty;
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
                    return (score.Name, chartConstant, difficulty, version, total);
                })
                .OrderByDescending(p => p.total)
                .ToList();

            if (scores.Count == 0)
            {
                await msg.DeleteAsync();
                return Reply("No score was found!");
            }
            
            var newScores = scores
                .Where(s => s.version == latestVersion)
                .Take(15)
                .ToList();
            var oldScores = scores
                .Where(s => s.version != latestVersion)
                .Take(35)
                .ToList();
            var newFooter = new LocalEmbedFooter()
                .WithText($"Total New rating : {NormalizeRating(newScores.Select(s => s.total).Sum())}");
            var oldFooter = new LocalEmbedFooter()
                .WithText($"Total Old rating : {NormalizeRating(oldScores.Select(s => s.total).Sum())}");

            var newEmbedFields = newScores
                .Chunk(2)
                .Select(pair =>
                {
                    var field = new LocalEmbedField()
                        .WithName(FormatScore(pair[0]));
                    return pair.Length > 1
                        ? field.WithValue(FormatScore(pair[1]))
                        : field.WithBlankValue();
                })
                .ToList();
            var newEmbed = new LocalEmbed
            {
                Title = "New charts - top rated plays",
                Fields = newEmbedFields,
                Footer = newFooter
            };

            var oldEmbed = oldScores
                .Chunk(20)
                .Select(page =>
                {
                    var entries = page.Chunk(2);
                    var fields = entries
                        .Select(pair =>
                        {
                            var field = new LocalEmbedField()
                                .WithName(FormatScore(pair[0]));
                            return pair.Length > 1
                                ? field.WithValue(FormatScore(pair[1]))
                                : field.WithBlankValue();
                        });
                    return new LocalEmbed
                    {
                        Title = "Old charts - top rated plays",
                        Fields = fields.ToList(),
                        Footer = oldFooter
                    };
                });

            var embeds = Enumerable.Empty<LocalEmbed>()
                .Append(newEmbed)
                .Concat(oldEmbed)
                .Select(embed => new Page().WithEmbeds(embed).WithContent("These calculations are estimate."));

            
            await msg.DeleteAsync();
            return View(new PagedView(new ListPageProvider(embeds)), TimeSpan.FromSeconds(30));
        }

        private static string FormatScore((string Name, int chartConstant, Difficulty difficulty, int version, long total) score)
        {
            var (name, constant, diff, _, total) = score;
            var diffText = diff.ToString()[..3].ToUpperInvariant();
            return $"[**{NormalizeRating(total), 3}**] ({diffText} {constant / 10, 2}.{constant % 10}) {name}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static int NormalizeRating(long total)
        {
            return (int) (total / 1000000 / 10 / 10);
        }
    }
}