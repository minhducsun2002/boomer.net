using System;
using System.Collections.Generic;
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
        public Top(HttpClient http, MaimaiDataService data, IMaimaiDxNetCookieProvider cookieProvider) : base(http, data, cookieProvider) { }

        private static readonly Difficulty[] Difficulties =
        {
            Difficulty.Basic, Difficulty.Advanced, Difficulty.Expert, Difficulty.Master, Difficulty.ReMaster
        };

        private static readonly string[] DifficultyStrings =
        {
            "BASIC", "ADVANCED", "EXPERT", "MASTER", "Re:MASTER"
        };

        [TextCommand("maitop")]
        [Description("Show top rated plays of an user.")]
        public async Task<IDiscordCommandResult> Exec(
            [Description("User in question")] IMember? player = null
        )
        {
            var cookie = await CookieProvider.GetCookie(player?.Id ?? Context.AuthorId);
            var client = new MaimaiDxNetClient(HttpClient, cookie!);

            var msg = await Reply($"A few seconds please, loading {Difficulties.Length} pages...");

            // Universe PLUS
            var latestVersion = GameDataService.NewestVersion == 0 ? 18 : GameDataService.NewestVersion;
            var records = Enumerable.Empty<ScoreRecord>();
            foreach (var diff in Difficulties)
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
                    return (score, version, chartConstant, total);
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

            var newEmbed = newScores
                .Chunk(10)
                .Select(entries =>
                {
                    var fields = CreateFields(entries);
                    return new LocalEmbed
                    {
                        Title = "New charts - top rated plays",
                        Fields = fields.ToList(),
                        Footer = newFooter
                    };
                });

            var oldEmbed = oldScores
                .Chunk(10)
                .Select(entries =>
                {
                    var fields = CreateFields(entries);
                    return new LocalEmbed
                    {
                        Title = "Old charts - top rated plays",
                        Fields = fields.ToList(),
                        Footer = oldFooter
                    };
                });

            var embeds = Enumerable.Empty<LocalEmbed>()
                .Concat(newEmbed)
                .Concat(oldEmbed)
                .Select(embed => new Page().WithEmbeds(embed).WithContent("These calculations are estimated."));


            await msg.DeleteAsync();
            return View(new PagedView(new ListPageProvider(embeds)), TimeSpan.FromSeconds(30));
        }

        private static IEnumerable<LocalEmbedField> CreateFields(
            IEnumerable<(ScoreRecord score, int version, int chartConstant, long total)> entries
        )
        {
            return entries
                .Select(entry =>
                {
                    var (score, _, constant, total) = entry;
                    var accuracy = score.Accuracy;
                    var diffText = DifficultyStrings[(int) score.Difficulty];
                    var field = new LocalEmbedField()
                        .WithName(
                            $"{score.Name}  [{diffText} {constant / 10}.{constant % 10}]")
                        .WithValue($"**{NormalizeRating(total),3}** rating - " +
                                   $"**{accuracy / 10000}**.**{(accuracy % 10000).ToString().PadRight(0, '0')}**%");
                    return field;
                });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static int NormalizeRating(long total)
        {
            return (int) (total / 1000000 / 10 / 10);
        }
    }
}