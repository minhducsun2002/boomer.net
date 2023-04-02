using Disqord.Bot.Commands;
using Disqord.Rest;
using Pepper.Commons.Maimai;
using Pepper.Commons.Structures.CommandAttributes.Metadata;
using Pepper.Frontends.Maimai.Database.MaimaiDxNetCookieProviders;
using Pepper.Frontends.Maimai.Services;
using Qmmands;
using Qmmands.Text;

namespace Pepper.Frontends.Maimai.Commands.Text
{
    [Hidden]
    [RequireBotOwner]
    public class Reload : MaimaiTextCommand
    {
        private readonly MaimaiDataDbContext dbContext;
        private readonly IHttpClientFactory httpClientFactory;
        public Reload(
            IHttpClientFactory httpClientFactory,
            MaimaiDxNetClientFactory factory,
            MaimaiDataService data, IMaimaiDxNetCookieProvider cookie,
            MaimaiDataDbContext dbContext
        ) : base(factory, data, cookie)
        {
            this.dbContext = dbContext;
            this.httpClientFactory = httpClientFactory;
        }

        [TextCommand("reload")]
        [Description("Flush cached song data & reload everything from database. Useful when editing stuff.")]
        public async Task<IDiscordCommandResult> Exec()
        {
            await Context.Message.AddReactionAsync(Hourglass);
            try
            {
                var client = httpClientFactory.CreateClient();
                await GameDataService.Load(dbContext, client, CancellationToken.None);
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