using Disqord.Rest;
using Pepper.Commons.Maimai;
using Pepper.Commons.Maimai.Structures;
using Pepper.Commons.Maimai.Structures.Score;
using Pepper.Frontends.Maimai.Database.MaimaiDxNetCookieProviders;
using Pepper.Frontends.Maimai.Services;

namespace Pepper.Frontends.Maimai.Commands
{
    public class TopScoreCommand : MaimaiCommand
    {
        protected TopScoreCommand(HttpClient http, MaimaiDataService data, IMaimaiDxNetCookieProvider cookieProvider)
            : base(http, data, cookieProvider) { }

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