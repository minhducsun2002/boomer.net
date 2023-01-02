using Disqord.Bot.Commands;
using Disqord.Rest;
using Pepper.Commons.Maimai;
using Pepper.Commons.Structures.CommandAttributes.Metadata;
using Pepper.Frontends.Maimai.Database.MaimaiDxNetCookieProviders;
using Pepper.Frontends.Maimai.Services;
using Qmmands;
using Qmmands.Text;

namespace Pepper.Frontends.Maimai.Commands
{
    [Hidden]
    [RequireBotOwner]
    public class Reload : MaimaiCommand
    {
        private readonly MaimaiDataDbContext dbContext;
        public Reload(
            HttpClient http, MaimaiDataService data, IMaimaiDxNetCookieProvider cookie,
            MaimaiDataDbContext dbContext
        ) : base(http, data, cookie)
        {
            this.dbContext = dbContext;
        }

        [TextCommand("reload")]
        [Description("Flush cached song data & reload everything from database. Useful when editing stuff.")]
        public async Task<IDiscordCommandResult> Exec()
        {
            await Context.Message.AddReactionAsync(Hourglass);
            try
            {
                await GameDataService.Load(dbContext, HttpClient, CancellationToken.None);
                await Context.Message.RemoveOwnReactionAsync(Hourglass);
                return Reaction(Success);
            }
            catch
            {
                await Context.Message.RemoveOwnReactionAsync(Hourglass);
                return Reaction(Failed);
            }
        }
    }
}