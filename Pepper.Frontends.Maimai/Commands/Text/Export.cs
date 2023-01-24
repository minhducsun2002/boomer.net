using System.IO.Compression;
using System.Text;
using System.Web;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Rest;
using Newtonsoft.Json;
using Pepper.Commons.Maimai;
using Pepper.Commons.Maimai.Entities;
using Pepper.Commons.Maimai.Structures.Data;
using Pepper.Commons.Maimai.Structures.Data.Score;
using Pepper.Commons.Structures.CommandAttributes.Metadata;
using Pepper.Frontends.Maimai.Database.MaimaiDxNetCookieProviders;
using Pepper.Frontends.Maimai.Services;
using Pepper.Frontends.Maimai.Structures;
using Pepper.Frontends.Maimai.Structures.Export;
using Pepper.Frontends.Maimai.Utils;
using Pepper.PngPayloadEmbed;
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
            using var imgStream = new MemoryStream(PngPayloadWrapper.Wrap(CreatePackedScores(sc)));
            msg = msg
                .AddAttachment(new LocalAttachment().WithStream(newStream).WithFileName("new.csv"))
                .AddAttachment(new LocalAttachment().WithStream(oldStream).WithFileName("old.csv"))
                .AddAttachment(new LocalAttachment().WithStream(imgStream).WithFileName("a.png"));

            var m = await Reply(msg);
            var a = m.Attachments.First(a => a.FileName.EndsWith("png")).Url;
            await m.ModifyAsync(m =>
            {
                m.Components = new[]
                {
                    LocalComponent.Row(
                        LocalComponent.LinkButton($"https://mai1.cipher.moe/?p={HttpUtility.UrlEncode(a)}", "try it and see")
                    )
                };
            });
            return null;
        }

        private byte[] CreatePackedScores(IEnumerable<ScoreWithMeta<TopRecord>> records)
        {
            var r = records
                .AsParallel()
                .WithDegreeOfParallelism(8)
                .Select(s =>
                {
                    var r = s.Score;
                    var levelDecimal = s.Difficulty?.LevelDecimal ?? (r.Level.Item2 ? 7 : 0);
                    return new TopExportScore
                    {
                        Accuracy = r.Accuracy,
                        ChartVersion = r.Version,
                        Difficulty = r.Difficulty,
                        DXScore = (r.Notes, r.MaxNotes),
                        FcStatus = r.FcStatus,
                        SyncStatus = r.SyncStatus,
                        Level = (r.Level.Item1, levelDecimal, s.Difficulty != null),
                        MaimaiVersion = GameDataService.NewestVersion,
                        Song = s.Score.Name
                    };
                });

            var res = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(r));
            using var memoryStream = new MemoryStream(res);
            using var outputStream = new MemoryStream();
            using var gzipStream = new GZipStream(outputStream, CompressionLevel.Optimal);
            memoryStream.CopyTo(gzipStream);
            return outputStream.ToArray();
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