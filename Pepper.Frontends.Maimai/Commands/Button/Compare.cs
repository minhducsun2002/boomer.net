using Disqord;
using Disqord.Bot.Commands.Components;
using Disqord.Rest;
using Pepper.Commons.Maimai;
using Pepper.Commons.Maimai.Entities;
using Pepper.Commons.Maimai.Structures;
using Pepper.Commons.Maimai.Structures.Enums;
using Pepper.Frontends.Maimai.Database.MaimaiDxNetCookieProviders;
using Pepper.Frontends.Maimai.Services;
using Pepper.Frontends.Maimai.Structures;
using Difficulty = Pepper.Commons.Maimai.Entities.Difficulty;
using DifficultyEnum = Pepper.Commons.Maimai.Structures.Enums.Difficulty;

namespace Pepper.Frontends.Maimai.Commands.Button
{
    public class Compare : MaimaiButtonCommand
    {
        public Compare(HttpClient httpClient, MaimaiDataService data, IMaimaiDxNetCookieProvider cookie)
            : base(httpClient, data, cookie) { }

        private const string Name = "maicompare_1";

        private static readonly DifficultyEnum[] DefaultDifficulties =
            { DifficultyEnum.Basic, DifficultyEnum.Advanced, DifficultyEnum.Expert, DifficultyEnum.Master };

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
            var client = new MaimaiDxNetClient(HttpClient, cookie!);
            var rec = await client.GetUserDifficultyRecord((baseLevel, plus == 1));
            var record = rec.FirstOrDefault(r => r.Name == name && r.Version == version);

            (Difficulty, Song)? p;
            if (baseLevel != 0)
            {
                p = id != 0
                    ? GameDataService.ResolveSongExact(id, difficulty)
                    : GameDataService.ResolveSongExact(name, difficulty, (baseLevel, plus != 0));
            }
            else
            {
                p = GameDataService.ResolveSongLoosely(name, difficulty, version);
            }
            var decimalLevel = p.HasValue
                ? p.Value.Item1.LevelDecimal
                : (plus == 1 ? 7 : 0);
            var title = $"**{name}**  [__{ScoreFormatter.DifficultyStrings[d]}__ **{baseLevel}**.**{decimalLevel}**]";

            if (record == null)
            {
                await Context.Interaction.Followup().SendAsync(
                    new LocalInteractionMessageResponse()
                        .WithContent($"No score for {Context.Author.Mention} on {title}")
                );
                return;
            }

            var chartRecord = await client.GetUserScoreOnChart(record.MusicDetailLink!);
            var detailedRecord = chartRecord.First(r => r.Difficulty == record.Difficulty);
            var image = GameDataService.GetImageUrl(record.Name);
            var multipleVersions = GameDataService.HasMultipleVersions(record.Name);
            var embed = ScoreFormatter.FormatScore(
                detailedRecord, p?.Item1, p?.Item2,
                levelHints: (p?.Item1.Level ?? baseLevel, plus == 1),
                imageUrl: image,
                hasMultipleVersions: multipleVersions
            );

            embed = embed.WithFooter("Click buttons below to check your score!");

            var orderedDifficulties = p?.Item2.Difficulties.OrderBy(d => d.Order).ToArray();
            var buttons = (orderedDifficulties?.Select(d => (DifficultyEnum) d.Order) ?? DefaultDifficulties)
                .Select(
                    (diff, index) =>
                    {
                        var diffRecord = orderedDifficulties?.ElementAtOrDefault(index);
                        var songDiff = diffRecord?.Level;
                        var songDecimal = diffRecord?.LevelDecimal;
                        return LocalComponent.Button(
                            CreateCommand(id, name, version, diff, songDiff, songDecimal == null ? null : songDecimal >= 7),
                            ScoreFormatter.DifficultyStrings[(int) diff]
                        ).WithStyle(LocalButtonComponentStyle.Secondary);
                    }
                )
                .Take(5);
            await Context.Interaction.Followup().SendAsync(
                new LocalInteractionMessageResponse()
                    .WithContent($"Score of {Context.Author.Mention}")
                    .WithEmbeds(embed)
                    // ReSharper disable once CoVariantArrayConversion
                    .WithComponents(LocalComponent.Row(buttons.ToArray()))
            );
        }

        public static string CreateCommand(int? id, string name, ChartVersion version, DifficultyEnum d, int? baseLevel, bool? plus)
        {
            return $"{Name}:{(id != 0 ? id : 0)}:{name}:{(int) version}:{(int) d}:{(baseLevel != 0 ? baseLevel : 0)}:{(plus is not true ? 0 : 1)}";
        }
    }
}