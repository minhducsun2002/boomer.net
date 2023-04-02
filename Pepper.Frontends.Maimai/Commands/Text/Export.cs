using System.Text;
using Disqord;
using Disqord.Bot.Commands;
using Pepper.Commons.Maimai;
using Pepper.Commons.Maimai.Entities;
using Pepper.Commons.Maimai.Structures.Data;
using Pepper.Commons.Maimai.Structures.Data.Score;
using Pepper.Commons.Structures.CommandAttributes.Metadata;
using Pepper.Frontends.Maimai.Database.MaimaiDxNetCookieProviders;
using Pepper.Frontends.Maimai.Services;
using Pepper.Frontends.Maimai.Utils;
using Qmmands;
using Qmmands.Text;

namespace Pepper.Frontends.Maimai.Commands.Text
{
    [Hidden]
    [RequireBotOwner]
    public class Export : TopScoreCommand
    {
        public Export(MaimaiDxNetClientFactory factory, MaimaiDataService data, IMaimaiDxNetCookieProvider cookieProvider)
            : base(factory, data, cookieProvider) { }

        [TextCommand("topcsv", "csv")]
        [Description("No description.")]
        public async Task<IDiscordCommandResult?> Exec(
            [Description("User in question")] IMember? player = null
        )
        {
            var cookie = await CookieProvider.GetCookie(player?.Id ?? Context.AuthorId);
            var client = ClientFactory.Create(cookie!);

            var records = await ListAllScores(client);

            var scores = records
                .AsParallel()
                .WithDegreeOfParallelism(8)
                .Select(score =>
                {
                    int chartConstant;
                    var searchRes = GameDataService.ResolveSongExact(score.Name, score.Difficulty, score.Level, score.Version);
                    var version = GameDataService.NewestVersion;
                    if (searchRes.HasValue)
                    {
                        var (diff, s) = searchRes.Value;
                        version = s.AddVersionId;
                        chartConstant = diff.Level * 10 + diff.LevelDecimal;
                    }
                    else
                    {
                        var diff = score.Level;
                        chartConstant = diff.Item1 * 10 + (diff.Item2 ? 7 : 0);
                    }

                    var total = Calculate.GetFinalScore(score.Accuracy, chartConstant);
                    Song? song = null;
                    if (searchRes.HasValue)
                    {
                        (_, song) = searchRes.Value;
                    }
                    return (score, chartConstant, song, total, version);
                })
                .GroupBy(s => s.version == LatestVersion)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(s => s.total).ToArray()
                );

            if (!scores.ContainsKey(true))
            {
                scores[true] = Array.Empty<(TopRecord score, int chartConstant, Song? song, long total, int version)>();
            }

            if (!scores.ContainsKey(false))
            {
                scores[false] = Array.Empty<(TopRecord score, int chartConstant, Song? song, long total, int version)>();
            }

            var msg = new LocalMessage();

            var newCsv = SerializeToCsv(scores[true]);
            var oldCsv = SerializeToCsv(scores[false]);
            var newBytes = Encoding.UTF8.GetBytes(newCsv);
            var oldBytes = Encoding.UTF8.GetBytes(oldCsv);
            using var newStream = new MemoryStream(newBytes);
            using var oldStream = new MemoryStream(oldBytes);
            msg = msg
                .AddAttachment(new LocalAttachment().WithStream(newStream).WithFileName("new.csv"))
                .AddAttachment(new LocalAttachment().WithStream(oldStream).WithFileName("old.csv"));

            await Reply(msg);
            return null;
        }

        private static string SerializeToCsv(IEnumerable<(TopRecord score, int chartConstant, Song? song, long total, int version)> list)
        {
            var l = list.Select(v =>
            {
                var (sc, constant, song, total, _) = v;
                return $"{song?.Genre?.Name ?? ""}|{sc.Name}|{sc.Difficulty.ToString()}|{constant}|{sc.Accuracy}|{total}";
            });
            return string.Join("\n", l);
        }
    }
}