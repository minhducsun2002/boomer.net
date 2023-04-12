using Disqord;
using Disqord.Bot.Commands;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Commons.Maimai.Structures.Data.Enums;
using Pepper.Frontends.MaimaiStatistics.Database.ProgressRecordProvider;
using Qmmands.Text;
using PagedView = Pepper.Commons.Structures.Views.PagedView;

namespace Pepper.Frontends.MaimaiStatistics.Commands
{
    public class Test : StatisticCommand
    {
        public Test(IProgressRecordProvider r) : base(r) { }
        private readonly int[] times = { 1, 7, 15, 30 };

        // TODO: Remove this command
        [TextCommand("woke")]
        public async Task<IDiscordCommandResult> Exec()
        {
            var now = DateTimeOffset.Now;
            var current = (await RecordProvider.ListMaxInRange()).ToDictionary(r => r.FriendId, r => r);
            var r = (await Task.WhenAll(
                    times.Select(async day =>
                    {
                        await using var r = Context.Bot.Services.CreateAsyncScope();
                        var rr = r.ServiceProvider.GetRequiredService<IProgressRecordProvider>();
                        var res = await rr.ListMaxInRange(null, now - TimeSpan.FromDays(day));
                        return res;
                    })
                ))
                .SelectMany(a => a)
                .GroupBy(a => a.FriendId)
                .ToList();
            r = r.OrderByDescending(r => r.MaxBy(p => p.Rating)!.Rating).ToList();
            var lines = r.Select(g =>
            {
                var n = g.OrderByDescending(g => g.Timestamp).ToList();
                var currentRating = current[n[0].FriendId].Rating;
                var ratings = g.Select(a => a.Rating)
                    .OrderByDescending(a => a)
                    .Select((a, i) => $"{times[i]}d : **{currentRating - a}**");
                var dan = n[0].Dan;
                var isShinDan = dan > 11;
                return new LocalEmbedField()
                    .WithName(
                        $"[{n[0].Rating}]  {n[0].Name}   - {(isShinDan ? "Shin " : "")}{(dan % 11).Ordinalize()} Dan - {(SeasonClass) n[0].Class}"
                    )
                    .WithValue(string.Join(" / ", ratings));
            });
            var embeds = lines.Chunk(10)
                .Select(chunk => new Page().WithEmbeds(new LocalEmbed().WithFields(chunk)));
            return View(new PagedView(new ListPageProvider(embeds)), TimeSpan.FromSeconds(30));
        }
    }
}