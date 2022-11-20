using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Disqord.Bot.Commands;
using Qmmands;
using Qmmands.Text;

namespace Pepper.Commands.General
{
    public class Rate : GeneralCommand
    {
        private static readonly SHA256 Hasher = SHA256.Create();

        [TextCommand("rate")]
        [Description("Rate something on a scale from 0 to 10.")]
        public IDiscordCommandResult Exec(
            [Remainder][Description("What do you want me to rate?")] string whatToRate = ""
        )
        {
            var obj = string.IsNullOrWhiteSpace(whatToRate)
                ? "you"
                : $"\"**{Regex.Replace(whatToRate, @"([|\\*])", @"\$1", RegexOptions.Compiled)}**\"";

            var hash = Hasher.ComputeHash(Encoding.UTF8.GetBytes(whatToRate));
            var result = hash.Aggregate(1, (current, @byte) => current + @byte) % 11;

            var msg = result switch
            {
                0 => "a big fat",
                1 or 2 => "quite a poor",
                3 or 4 => "an improvable",
                5 or 6 => "a somewhat moderate",
                7 => "an acceptable",
                8 => "a high",
                9 => "an excellent",
                10 => "a solid",
                _ => "a mild"
            };

            return Reply($"I'd give {obj} {msg} **{result}/10**.");
        }
    }
}