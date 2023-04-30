using Disqord;
using Disqord.Bot.Commands.Components;
using Disqord.Rest;
using Humanizer;
using Pepper.Commons.Maimai;
using Pepper.Commons.Maimai.Entities;
using Pepper.Commons.Maimai.Structures.Data.Enums;
using Pepper.Commons.Maimai.Structures.Data.Score;
using Pepper.Frontends.Maimai.Database.MaimaiDxNetCookieProviders;
using Pepper.Frontends.Maimai.Services;
using Pepper.Frontends.Maimai.Structures;
using Pepper.Frontends.Maimai.Utils;
using Difficulty = Pepper.Commons.Maimai.Entities.Difficulty;
using DifficultyEnum = Pepper.Commons.Maimai.Structures.Data.Enums.Difficulty;

namespace Pepper.Frontends.Maimai.Commands.Button
{
    public class Compare : MaimaiButtonCommand
    {
        public Compare(MaimaiDxNetClientFactory factory, MaimaiDataService data, IMaimaiDxNetCookieProvider cookie)
            : base(factory, data, cookie) { }

        private const string Name = "maicompare_1";

        private const string GuideText = "Click buttons below to check your score!";

        [ButtonCommand($"{Name}:*:*:*:*:*:*")]
        // id, name, dx/std, bas/adv/exp/mas/remas, 13, + or not
        public async Task Exec(int id, string name, int ver, int d, int baseLevel, int plus)
        {
            // validate stuff
            if (!Enum.IsDefined((ChartVersion) ver) || !Enum.IsDefined((DifficultyEnum) d)
                                                        || (plus != 1 && plus != 0) /* true/false */)
            {
                return;
            }
            var difficulty = (DifficultyEnum) d;
            var version = (ChartVersion) ver;

            await Context.Interaction.Response().DeferAsync();

            var cookie = await CookieProvider.GetCookie(Context.AuthorId);
            var client = ClientFactory.Create(cookie!);
            var rec = await client.GetUserDifficultyMetadata(DifficultyEnum.Basic);
            var record = rec.FirstOrDefault(r => r.Name == name && r.Version == version);
            if (record == null)
            {
                await Context.Interaction.Followup().SendAsync(
                    new LocalInteractionMessageResponse()
                        .WithContent($"No score for {Context.Author.Mention} on **{name}**.\n{GuideText}")
                );
                return;
            }

            var chartRecord = await client.GetUserScoreOnChart(record.MusicDetailLink!);
            var image = GameDataService.GetImageUrl(record.Name);
            var detailedRecord = chartRecord
                .OrderBy(r => r.Difficulty)
                .Select(r =>
                {
                    var s = GameDataService.ResolveSongExact(r.Name, r.Difficulty, r.Level, r.Version);
                    var song = s?.Item2;
                    var multiple = GameDataService.HasMultipleVersions(r.Name);
                    return new ScoreWithMeta<ChartRecord>(
                        r,
                        song,
                        s?.Item1,
                        song?.AddVersionId ?? GameDataService.NewestVersion,
                        multiple,
                        image
                    );
                })
                .ToArray();
            var embeds = detailedRecord.Select(r =>
            {
                var levelText = Format.ChartConstant(r);
                var diffText = ScoreFormatter.DifficultyStrings[(int) r.Score.Difficulty];
                var playCount = r.Score.PlayCount;
                var last = r.Score.LastPlayed;

                var embed = new LocalEmbed()
                    .WithAuthor(Format.SongName(detailedRecord[0]) + " [" + diffText + " " + levelText + "]")
                    .WithColor(Format.Color(r.Score.Difficulty))
                    .WithDescription(
                        $"{Format.Statistics(r.Score)} - {Format.Rating(r)} rating" +
                        $"\n\n{playCount} {(playCount < 2 ? "play" : "play".Pluralize())}, last played <t:{last.ToUnixTimeSeconds()}:f>"
                    );
                if (image is not null)
                {
                    embed = embed.WithThumbnailUrl(image);
                }

                return embed;
            });

            var meta = detailedRecord[0];
            var metaSc = meta.Score;
            await Context.Interaction.Followup().SendAsync(
                new LocalInteractionMessageResponse()
                    .WithContent($"Score of {Context.Author.Mention}")
                    .WithEmbeds(embeds)
                    // ReSharper disable once CoVariantArrayConversion
                    .WithComponents(
                        LocalComponent.Row(
                            LocalComponent.Button(
                                CreateCommand(meta.Song?.Id, metaSc.Name, metaSc.Version, metaSc.Difficulty, 0, null),
                                "Check your score"
                            ).WithStyle(LocalButtonComponentStyle.Success)
                        )
                    )
            );
        }

        public static string CreateCommand(int? id, string name, ChartVersion version, DifficultyEnum d, int? baseLevel, bool? plus)
        {
            return $"{Name}:{(id != 0 ? id : 0)}:{name}:{(int) version}:{(int) d}:{(baseLevel != 0 ? baseLevel : 0)}:{(plus is not true ? 0 : 1)}";
        }
    }
}