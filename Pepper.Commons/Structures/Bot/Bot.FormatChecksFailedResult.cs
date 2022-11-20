using Disqord;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Text;
using Qmmands;

namespace Pepper.Commons.Structures
{
    public partial class Bot
    {
        private bool FormatChecksFailedResult(IDiscordTextCommandContext context, LocalMessageBase messageBase, ChecksFailedResult result)
        {
            var checks = result.FailedChecks;
            var types = checks.Keys.ToDictionary(c => c.GetType());

            if (types.ContainsKey(typeof(RequireBotOwnerAttribute)))
            {
                messageBase.Content = "Sorry, only my owner can do that.";
                return true;
            }

            if (types.ContainsKey(typeof(RequirePrivateAttribute)))
            {
                messageBase.Content = "Please send this via DM instead.";
                return true;
            }

            var firstValidCheck = checks.First().Key;
            messageBase.Content = @$"A secret check named {
                firstValidCheck.GetType().Name.Replace("Attribute", "", StringComparison.OrdinalIgnoreCase)} failed.";
            return true;
        }
    }
}