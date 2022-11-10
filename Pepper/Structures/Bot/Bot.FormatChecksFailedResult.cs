using System;
using System.Linq;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Text;
using Pepper.Structures.CommandAttributes.Checks;
using Qmmands;

namespace Pepper.Structures
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

            if (types.ContainsKey(typeof(RequireGuildWhitelistAttribute)))
            {
                messageBase.Content = "Guild-restricted command.";
                return true;
            }

            if (types.ContainsKey(typeof(RequirePrivateAttribute)))
            {
                messageBase.Content = "Please send this via DM instead.";
                return true;
            }

            var prefixCheckType = typeof(PrefixCheckAttribute);
            if (types.Count == 1 && types.First().Key == prefixCheckType)
            {
                return false;
            }

            var firstValidCheck = checks.First(c => c.GetType() != prefixCheckType).Key;
            messageBase.Content = @$"A secret check named {
                firstValidCheck.GetType().Name.Replace("Attribute", "", StringComparison.OrdinalIgnoreCase)} failed.";
            return true;
        }
    }
}