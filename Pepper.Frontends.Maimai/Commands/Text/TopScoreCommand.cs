using Disqord.Rest;
using Pepper.Commons.Maimai;
using Pepper.Commons.Maimai.Structures.Data;
using Pepper.Commons.Maimai.Structures.Data.Score;
using Pepper.Frontends.Maimai.Database.MaimaiDxNetCookieProviders;
using Pepper.Frontends.Maimai.Services;

namespace Pepper.Frontends.Maimai.Commands.Text
{
    public class TopScoreCommand : MaimaiTextCommand
    {
        protected TopScoreCommand(MaimaiDxNetClientFactory factory, MaimaiDataService data, IMaimaiDxNetCookieProvider cookieProvider)
            : base(factory, data, cookieProvider) { }

        public override async ValueTask OnBeforeExecuted()
        {
            await Context.Message.AddReactionAsync(Hourglass);
        }

        public override async ValueTask OnAfterExecuted()
        {
            await Context.Message.RemoveOwnReactionAsync(Hourglass);
        }

        protected async Task<IEnumerable<TopRecord>> ListAllScores(MaimaiDxNetClient client)
        {
            var records = Enumerable.Empty<TopRecord>();
            foreach (var diff in Difficulties)
            {
                records = records.Concat(await client.GetUserDifficultyRecord(diff));
            }

            return records;
        }
    }
}