using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Gateway;
using Disqord.Rest;
using osu.Game.Rulesets;
using Pepper.Commons.Osu;
using Pepper.Commons.Structures;
using Pepper.Database.OsuUsernameProviders;
using Pepper.Services.Osu;
using Pepper.Structures;
using Pepper.Structures.CommandAttributes.Checks;
using Pepper.Structures.External.Osu.Extensions;
using Qmmands;
using Qmmands.Text;
using PagedView = Pepper.Commons.Structures.Views.PagedView;

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
        [TextCommand("rank", "ranks")]
        [Description("See your ranking compared to other players in this server")]
        public async Task<IDiscordCommandResult> Exec([Flag("/")][Description("Game mode to check. Default to osu!.")] Ruleset ruleset)
        {
            var context = (IDiscordGuildCommandContext) Context;
            var guild = await context.Bot.FetchGuildAsync(context.GuildId);
            var cachedMembers = guild!.GetMembers().Values;

            var msg = await Reply("Please wait a bit. I'm collecting usernames & stats. Hang tight...");
            var records = await usernameProvider.GetUsernamesBulk(cachedMembers.Select(member => member.Id.ToString()).ToArray());

            if (records.Count == 0)
            {
                return Reply("No registered users in this guild.");
            }

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
                                Name = $"{user.Username} ({server.GetDisplayText()})",
                                Value = $"**{user.Statistics.PP:F2}**pp • **{user.Statistics.Accuracy:F3}**%"
                                        + $" • {ResolveEarthEmoji(user.CountryCode.ToString())} #{user.Statistics.GlobalRank}"
                                        + $" • :flag_{user.CountryCode.ToString().ToLowerInvariant()}: #{user.Statistics.CountryRank}"
                                        + $"\n<@{key}>"
                            };
                        }).ToList()
                    };
                    return new Page().WithEmbeds(embed);
                }).ToList();

            await msg.DeleteAsync();

            return pages.Count == 1
                ? Reply(pages[0].Embeds.Value[0])
                : Menu(new DefaultTextMenu(new PagedView(new ListPageProvider(pages))));
        }
    }
}