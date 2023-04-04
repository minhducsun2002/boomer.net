using Disqord.Bot.Commands;
using Pepper.Commons.Maimai;
using Pepper.Commons.Structures.CommandAttributes.Metadata;
using Pepper.Frontends.Maimai.Commands.Text;
using Pepper.Frontends.Maimai.Database.MaimaiDxNetCookieProviders;
using Pepper.Frontends.Maimai.Services;
using Qmmands;
using Qmmands.Text;

namespace Pepper.Frontends.Maimai.Commands.Debugging
{
    [RequireBotOwner]
    [Category("Debugging")]
    [Hidden]
    public class Flush : MaimaiTextCommand
    {
        public Flush(MaimaiDxNetClientFactory f, MaimaiDataService d, IMaimaiDxNetCookieProvider c) : base(f, d, c) { }

        [TextCommand("flush")]
        [Description("Remove cached cookies (for an user if specified)")]
        public IDiscordCommandResult Exec([Description("User ID to remove cache")] ulong? discordId = null)
        {
            CookieProvider.FlushCache(discordId);
            return Reply(discordId == null ? "Flushed cache for all users." : $"Flushed cache for user {discordId}.");
        }
    }
}