using Pepper.Commons.Maimai;
using Pepper.Commons.Maimai.Structures;
using Pepper.Frontends.Maimai.Database.MaimaiDxNetCookieProviders;
using Pepper.Frontends.Maimai.Services;

namespace Pepper.Frontends.Maimai.Commands
{
    public class TopScoreCommand : MaimaiCommand
    {
        public TopScoreCommand(HttpClient http, MaimaiDataService data, IMaimaiDxNetCookieProvider cookieProvider)
            : base(http, data, cookieProvider) { }

        protected async Task<IEnumerable<ScoreRecord>> ListAllScores(MaimaiDxNetClient client)
        {
            var records = Enumerable.Empty<ScoreRecord>();
            foreach (var diff in Difficulties)
            {
                records = records.Concat(await client.GetUserDifficultyRecord(diff));
            }

            return records;
        }
    }
}