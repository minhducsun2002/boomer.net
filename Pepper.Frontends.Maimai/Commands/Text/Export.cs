using System.Text;
using Disqord;
using Disqord.Bot.Commands;
using Pepper.Commons.Maimai;
using Pepper.Commons.Maimai.Structures.Data.Score;
using Pepper.Commons.Structures.CommandAttributes.Metadata;
using Pepper.Frontends.Maimai.Database.MaimaiDxNetCookieProviders;
using Pepper.Frontends.Maimai.Services;
using Pepper.Frontends.Maimai.Structures;
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

            var sc = records
                .AsParallel()
                .WithDegreeOfParallelism(8)
                .Select(score =>
                {
                    var searchRes =
                        GameDataService.ResolveSongExact(score.Name, score.Difficulty, score.Level, score.Version);
                    var a = new ScoreWithMeta<TopRecord>(
                        score,
                        searchRes?.Item2,
                        searchRes?.Item1,
                        GameDataService.NewestVersion,
                        GameDataService.HasMultipleVersions(score.Name),
                        GameDataService.GetImageUrl(score.Name));
                    return a;
                })
                .ToArray();
            var scores = sc
                .GroupBy(s => s.Version == LatestVersion)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(s => s.Rating).ToArray()
                );

            if (!scores.ContainsKey(true))
            {
                scores[true] = Array.Empty<ScoreWithMeta<TopRecord>>();
            }

            if (!scores.ContainsKey(false))
            {
                scores[false] = Array.Empty<ScoreWithMeta<TopRecord>>();
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

        private static string SerializeToCsv(IEnumerable<ScoreWithMeta<TopRecord>> list)
        {
            var l = list.Select(v =>
            {
                var s = v.Song;
                var sc = v.Score;
                return $"{s?.Genre?.Name ?? ""}|{sc.Name}|{v.Score.Difficulty}|{v.ChartConstant!.Value}|{sc.Accuracy}|{v.Rating!.Value}";
            });
            return string.Join("\n", l);
        }
    }
}