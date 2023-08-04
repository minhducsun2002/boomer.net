using Disqord;
using Disqord.Bot.Commands;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Commons.Maimai.Structures.Data.Enums;
using Pepper.Frontends.MaimaiStatistics.Database.ProgressRecordProvider;
using Pepper.Frontends.MaimaiStatistics.Structures;
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
            var current = (await RecordProvider.ListMaxAllTime())
                .ToDictionary(r => r.FriendId, r => r)
                .OrderByDescending(a => a.Value.Rating)
                .ToArray();
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
                .ToDictionary(a => a.Key, a => a.ToArray());

            var lines = current.Select(v =>
            {
                var (friend, latest) = v;
                if (!r.TryGetValue(friend, out var g))
                {
                    g = Array.Empty<ProgressRecord>();
                }
                var currentRating = latest.Rating;
                var ratings = g
                    .OrderByDescending(a => a.Timestamp)
                    .Select(a => a.Rating)
                    .Select((a, i) => $"{times[i]}d : **{(currentRating - a <= 0 ? "" : "+")}{currentRating - a}**");
                var joined = string.Join(" / ", ratings);
                var dan = latest.Dan;
                return new LocalEmbedField()
                    .WithName(
                        $"[{latest.Rating}]  {latest.Name}   - {(dan > 11 ? "Shin " : "")}{(dan % 11).Ordinalize()} Dan - {(SeasonClass) latest.Class}"
                    )
                    .WithValue(string.IsNullOrWhiteSpace(joined) ? "(not enough data)" : joined);
            });
            var embeds = lines.Chunk(10)
                .Select(chunk => new Page().WithEmbeds(new LocalEmbed().WithFields(chunk)));
            return View(new PagedView(new ListPageProvider(embeds)), TimeSpan.FromSeconds(30));
        }
    }
}