using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Rest;
using Pepper.Commons.Maimai;
using Pepper.Commons.Maimai.Structures.Data.Score;
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
    public class Dump : TopScoreCommand
    {
        public Dump(MaimaiDxNetClientFactory f, MaimaiDataService d, IMaimaiDxNetCookieProvider c) : base(f, d, c) { }

        [TextCommand("dump")]
        [Description("Provide a top play dump and a link to website displaying its content.")]
        public async Task<IDiscordCommandResult?> Exec(
            [Description("User in question")] IMember? player = null
        )
        {
            var cookie = await CookieProvider.GetCookie(player?.Id ?? Context.AuthorId);
            var client = ClientFactory.Create(cookie!);

            var user = await client.GetUserPlayData();
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
            var serializedDump = PngPayloadWrapper.Wrap(Encrypt(CreatePackedScores(sc, user)));
            using var imgStream = new MemoryStream(serializedDump);
            var msg = new LocalMessage().AddAttachment(new LocalAttachment().WithStream(imgStream).WithFileName("a.png"));
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

        private byte[] Encrypt(byte[] payload)
        {
            var password = Environment.GetEnvironmentVariable("DUMP_PASSWORD") ?? "";
            var key = SHA256.HashData(Encoding.UTF8.GetBytes(password))[..16];
            var aes = Aes.Create();
            aes.KeySize = 128;
            aes.BlockSize = 128;
            aes.GenerateIV();
            aes.Key = key;
            var res = aes.EncryptCbc(payload, aes.IV, PaddingMode.PKCS7);
            var final = aes.IV.Concat(res).ToArray();
            return final;
        }

        private byte[] CreatePackedScores(IEnumerable<ScoreWithMeta<TopRecord>> records, Commons.Maimai.Structures.Data.User? user = null)
        {
            var scores = records
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
                        DxScore = r.Notes,
                        MaxDxScore = r.MaxNotes,
                        FcStatus = r.FcStatus,
                        SyncStatus = r.SyncStatus,
                        Level = r.Level.Item1,
                        LevelDecimal = levelDecimal,
                        TrueDecimal = s.Difficulty != null,
                        MaimaiVersion = s.Song?.AddVersionId ?? GameDataService.NewestVersion,
                        Song = s.Score.Name,
                        GenreId = s.Song?.GenreId ?? -1,
                        Genre = s.Score.Genre
                    };
                })
                .ToArray();

            TopExportUser? u = null;
            if (user is not null)
            {
                u = new TopExportUser
                {
                    Name = user.Name,
                    Rating = user.Rating,
                    DanLevel = user.DanLevel,
                    PlayCount = user.PlayCount
                };
            }

            var genres = GameDataService.GenreCache.Select(kv => new TopExportGenre
            {
                Id = kv.Key,
                Name = kv.Value
            });

            var export = new TopExport
            {
                Coefficients = Calculate.Coeff.Select(a => new[] { a.Item1, a.Item2 }).ToArray(),
                MaimaiVersion = GameDataService.NewestVersion,
                Timestamp = DateTimeOffset.Now,
                TopExportScores = scores,
                User = u,
                TopExportGenres = genres.ToArray()
            };

            var res = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(export));
            using var memoryStream = new MemoryStream(res);
            using var outputStream = new MemoryStream();
            // Apparently GzipStream only flushes everything upon disposal, so at the .ToArray() call it must have been disposed
            using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress, true))
            {
                memoryStream.CopyTo(gzipStream);
            }
            return outputStream.ToArray();
        }
    }
}