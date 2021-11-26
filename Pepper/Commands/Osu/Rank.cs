using System.Linq;
using System.Threading.Tasks;
using BAMCIS.ChunkExtensionMethod;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.DependencyInjection;
using osu.Game.Rulesets;
using Pepper.Services.Osu;
using Pepper.Structures.Commands;
using Qmmands;
using PagedView = Pepper.Structures.PagedView;

namespace Pepper.Commands.Osu
{
    [RequireGuild]
    public class Rank : BeatmapContextCommand
    {
        public Rank(APIService service, BeatmapContextProviderService b) : base(service, b) { }

        [RequireGuildWhitelist("osu-leaderboard")]
        [Command("rank", "ranks")]
        [Description("See your ranking compared to other players in this server")]
        public async Task<DiscordCommandResult> Exec([Flag("/")][Description("Game mode to check. Default to osu!.")] Ruleset ruleset)
        {
            var service = Context.Services.GetRequiredService<DiscordOsuUsernameLookupService>();
            var context = (DiscordGuildCommandContext) Context;
            var cachedMembers = context.Guild.GetMembers().Values;

            var msg = await Reply("Please wait a bit. I'm collecting usernames & stats. Hang tight...");
            var records = await service.GetManyUsers(cachedMembers.Select(member => (ulong) member.Id).ToArray());
            var profiles = await Task.WhenAll(
                records
                    .Select(async kv => (kv.Key, await APIService.GetUser(kv.Value, ruleset.RulesetInfo)))
            );

            var pages = profiles
                .OrderBy(profile => profile.Item2.Item1.Statistics.GlobalRank)
                .Chunk(10)
                .Select(chunk =>
                {
                    var embed = new LocalEmbed
                    {
                        Fields = chunk.Select(profile =>
                        {
                            var (key, userTuple) = profile;
                            var user = userTuple.Item1;
                            return new LocalEmbedField
                            {
                                Name = user.Username,
                                Value = $"**{user.Statistics.PP:F2}**pp • **{user.Statistics.Accuracy:F3}**%"
                                        + $" • {ResolveEarthEmoji(user.Country.FlagName)} #{user.Statistics.GlobalRank}"
                                        + $" • :flag_{user.Country.FlagName.ToLowerInvariant()}: #{user.Statistics.CountryRank}"
                                        + $"\n<@{key}>"
                            };
                        }).ToList()
                    };
                    return new Page().WithEmbeds(embed);
                }).ToList();

            await msg.DeleteAsync();

            if (pages.Count == 0)
            {
                return Reply("No registered users in this guild.");
            }

            return pages.Count == 1
                ? Reply(pages[0].Embeds[0])
                : View(new PagedView(new ListPageProvider(pages)));
        }
    }
}