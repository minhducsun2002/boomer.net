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
using Pepper.Commons.Osu;
using Pepper.Database.OsuUsernameProviders;
using Pepper.Services.Osu;
using Pepper.Structures.Commands;
using Pepper.Structures.External.Osu.Extensions;
using Qmmands;
using PagedView = Pepper.Structures.PagedView;

namespace Pepper.Commands.Osu
{
    [RequireGuild]
    public class Rank : BeatmapContextCommand
    {
        private readonly IOsuUsernameProvider usernameProvider;

        public Rank(APIClientStore s, BeatmapContextProviderService b, IOsuUsernameProvider usernameProvider) : base(s, b)
        {
            this.usernameProvider = usernameProvider;
        }

        [RequireGuildWhitelist("osu-leaderboard")]
        [Command("rank", "ranks")]
        [Description("See your ranking compared to other players in this server")]
        public async Task<DiscordCommandResult> Exec([Flag("/")][Description("Game mode to check. Default to osu!.")] Ruleset ruleset)
        {
            var context = (DiscordGuildCommandContext) Context;
            var cachedMembers = context.Guild.GetMembers().Values;

            var msg = await Reply("Please wait a bit. I'm collecting usernames & stats. Hang tight...");
            var records = await usernameProvider.GetUsernamesBulk(cachedMembers.Select(member => member.Id.ToString()).ToArray());
            var profiles = await Task.WhenAll(
                records
                    .Select(async kv =>
                    {
                        var (userId, usernameRecord) = kv;
                        var server = usernameRecord.DefaultServer;
                        var apiClient = APIClientStore.GetClient(server);
                        var user = await apiClient.GetUser(usernameRecord.GetUsername(server)!, ruleset.RulesetInfo);
                        return (userId, user, server);
                    })
            );

            var pages = profiles
                .OrderBy(profile => profile.Item2.Statistics.GlobalRank)
                .Chunk(10)
                .Select(chunk =>
                {
                    var embed = new LocalEmbed
                    {
                        Fields = chunk.Select(profile =>
                        {
                            var (key, user, server) = profile;
                            return new LocalEmbedField
                            {
                                Name = $"{user.Username} ({server.GetDisplayText()}",
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