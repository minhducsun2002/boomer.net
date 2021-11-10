using System.Drawing;
using System.Threading.Tasks;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Users;
using Pepper.Services.Osu.API;
using APIScoreInfo = Pepper.Commons.Osu.API.APIScoreInfo;

namespace Pepper.Services.Osu
{
    public partial class APIService
    {
        private readonly OsuUserCache userCache = new();

        public async Task<(APIUser, APIScoreInfo[], Color)> GetUser(string username, RulesetInfo rulesetInfo)
        {
            var (user, scores) = await userCache.Get(rulesetInfo: rulesetInfo, username: username);
            var color = await userCache.GetUserAvatarDominantColor(user);
            return (user, scores, color);
        }
    }
}