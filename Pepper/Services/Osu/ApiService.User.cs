using System.Drawing;
using System.Threading.Tasks;
using osu.Game.Rulesets;
using osu.Game.Users;
using Pepper.Commons.Osu.API;
using Pepper.Services.Osu.API;

namespace Pepper.Services.Osu
{
    public partial class APIService
    {
        private readonly OsuUserCache userCache = new();

        public async Task<(User, APILegacyScoreInfo[], Color)> GetUser(string username, RulesetInfo rulesetInfo)
        {
            var (user, scores) = await userCache.Get(rulesetInfo: rulesetInfo, username: username);
            var color = await userCache.GetUserAvatarDominantColor(user);
            return (user, scores, color);
        }
    }
}