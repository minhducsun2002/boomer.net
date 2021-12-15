using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pepper.Database.OsuUsernameProviders
{
    public interface IOsuUsernameProvider
    {
        public ValueTask<Username?> GetUsernames(string discordId);
        public ValueTask<Dictionary<string, Username>> GetUsernamesBulk(params string[] discordUserId);
        public Task<Username> StoreUsername(Username record);
    }
}