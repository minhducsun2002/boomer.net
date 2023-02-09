using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Disqord;
using Disqord.Bot.Commands;
using Qmmands;
using Qmmands.Text;

namespace Pepper.Commons.Commands.General
{
    public class Rate : GeneralCommand
    {
        private static readonly SHA256 Hasher = SHA256.Create();

        private const int RngBucketSize = 86400 / 2;
        private const int ImprovementBucketSize = 86400 * 3 / 2;

        // -0.75 to 0.5
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static double GetRngAdjustment(long unixTimestamp)
        {
            var bucket = unixTimestamp / RngBucketSize;
            double part15 = (double) bucket / 15, part2 = (double) bucket / 2;
            var p1 = Math.Sin(part15);
            var p2 = Math.Cos(part2);
            var p3 = p1 * p1 * p2 * 5 / 8;
            var p4 = p3 - 0.125;
            return p4;
        }

        // 0 to 0.1
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static double GetImprovementRate(long unixTimestamp)
        {
            var bucket = unixTimestamp / ImprovementBucketSize;
            double part3 = (double) bucket / 3, part2 = (double) bucket / 2;
            double cos3 = Math.Cos(part3), cos2 = Math.Cos(part2);
            var p1 = cos3 * cos2;
            return Math.Abs(p1) / 10;
        }

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
            var timestamp = Context.Message.CreatedAt().ToUnixTimeSeconds();

            var baseRating = hash.Aggregate(1, (current, @byte) => current + @byte) % 11;
            var improvement = GetImprovementRate(timestamp) + GetImprovementRate((long) (Context.AuthorId.RawValue / 2));
            if (Math.Abs(improvement) > 1)
            {
                improvement /= 2;
            }
            var burst = (GetRngAdjustment(timestamp) + GetRngAdjustment((long) (Context.AuthorId.RawValue / 2))) / 2;
            var scaled = (int) Math.Round((baseRating + improvement * baseRating + burst) * 100);
            if (scaled % 100 >= 75)
            {
                scaled = Math.Min(scaled + 100, 1100);
            }
            var result = scaled / 100;
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
                11 => "a flawless",
                _ => "a mild"
            };

            return Reply($"I'd give {obj} {msg} **{result}/10**.");
        }
    }
}