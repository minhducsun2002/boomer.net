using Disqord;
using Disqord.Bot.Commands;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Rest;
using Pepper.Commons.Maimai;
using Pepper.Commons.Maimai.Entities;
using Pepper.Commons.Maimai.Structures.Data;
using Pepper.Commons.Maimai.Structures.Data.Enums;
using Pepper.Commons.Maimai.Structures.Data.Score;
using Pepper.Frontends.Maimai.Database.MaimaiDxNetCookieProviders;
using Pepper.Frontends.Maimai.Services;
using Pepper.Frontends.Maimai.Structures;
using Pepper.Frontends.Maimai.Utils;
using Qmmands;
using Qmmands.Text;
using PagedView = Pepper.Commons.Structures.Views.PagedView;

namespace Pepper.Frontends.Maimai.Commands.Text
{
    public class Top : TopScoreCommand
    {
        public Top(MaimaiDxNetClientFactory factory, MaimaiDataService data, IMaimaiDxNetCookieProvider cookieProvider) : base(factory, data, cookieProvider) { }

        [TextCommand("maitop", "top")]
        [Description("Show top rated plays of an user.")]
        public async Task<IDiscordCommandResult> Exec(
            [Description("User in question")] IMember? player = null
        )
        {
            var cookie = await CookieProvider.GetCookie(player?.Id ?? Context.AuthorId);
            var client = ClientFactory.Create(cookie);

            var records = await ListAllScores(client);

            var scores = records
                .AsParallel()
                .WithDegreeOfParallelism(8)
                .Select(score =>
                {
                    var searchRes = GameDataService.ResolveSongExact(score.Name, score.Difficulty, score.Level, score.Version);
                    var a = new ScoreWithMeta<TopRecord>(
                        score,
                        searchRes?.Item2,
                        searchRes?.Item1,
                        GameDataService.NewestVersion,
                        GameDataService.HasMultipleVersions(score.Name),
                        GameDataService.GetImageUrl(score.Name));
                    return a;
                })
                .Where(p => p.Rating != null)
                .OrderByDescending(p => p.Rating)
                .ThenByDescending(p => p.ChartConstant)
                .ThenByDescending(p => p.Score.Accuracy)
                .ToList();

            if (scores.Count == 0)
            {
                return Reply("No score was found!");
            }

            var newScores = scores
                .Where(s => s.AddVersion == LatestVersion)
                .Take(15)
                .ToList();
            var oldScores = scores
                .Where(s => s.AddVersion != LatestVersion)
                .Take(35)
                .ToList();

            return View(new TopScorePagedView(new TopScorePageProvider(newScores, oldScores)), TimeSpan.FromSeconds(30));
        }
    }
}