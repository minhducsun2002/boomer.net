using Disqord;
using Disqord.Bot.Commands.Components;
using Disqord.Rest;
using Pepper.Commons.Maimai;
using Pepper.Commons.Maimai.Entities;
using Pepper.Commons.Maimai.Structures;
using Pepper.Frontends.Maimai.Database.MaimaiDxNetCookieProviders;
using Pepper.Frontends.Maimai.Services;
using Difficulty = Pepper.Commons.Maimai.Entities.Difficulty;
using DifficultyEnum = Pepper.Commons.Maimai.Structures.Difficulty;

namespace Pepper.Frontends.Maimai.Commands.Button
{
    public class Compare : MaimaiButtonCommand
    {
        public Compare(HttpClient httpClient, MaimaiDataService data, IMaimaiDxNetCookieProvider cookie)
            : base(httpClient, data, cookie) { }

        private const string Name = "maicompare_1";

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
            var title = $"**{name}**  [__{MaimaiCommand.DifficultyStrings[d]}__ **{baseLevel}**.**{decimalLevel}**]";

            if (record == null)
            {
                await Context.Interaction.Followup().SendAsync(
                    new LocalInteractionMessageResponse()
                        .WithContent($"No score for {Context.Author.Mention} on {title}")
                );
                return;
            }

            var score = MaimaiCommand.GetFinalScore(record.Accuracy, baseLevel * 10 + decimalLevel);
            await Context.Interaction.Followup().SendAsync(
                new LocalInteractionMessageResponse()
                    .WithContent($"Score of {Context.Author.Mention}\n" +
                                 $"{title} : **{record.Accuracy / 10000}**.**{record.Accuracy % 10000:0000}**% - {MaimaiCommand.NormalizeRating(score)}")
            );
        }

        public static string CreateCommand(int? id, string name, ChartVersion version, DifficultyEnum d, int? baseLevel, bool? plus)
        {
            return $"{Name}:{(id != 0 ? id : 0)}:{name}:{(int) version}:{(int) d}:{(baseLevel != 0 ? baseLevel : 0)}:{(plus is not true ? 0 : 1)}";
        }
    }
}