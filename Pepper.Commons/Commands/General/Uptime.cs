using System.Diagnostics;
using System.Runtime.InteropServices;
using Disqord;
using Disqord.Bot.Commands;
using Humanizer;
using Qmmands;
using Qmmands.Text;

namespace Pepper.Commons.Commands.General
{
    public class Uptime : GeneralCommand
    {
        [TextCommand("uptime")]
        [Description("How long has I been running?")]
        public IDiscordCommandResult Exec()
        {
            var threads = Process.GetCurrentProcess().Threads.Cast<ProcessThread>().Select(thread =>
            {
                try
                {
                    // TODO : Do something with this? This might break on Macs.
#pragma warning disable CA1416
                    var interval = DateTime.Now - thread.StartTime;
#pragma warning restore CA1416
                    var timeComponents = new[]
                        {
                            (interval.Days, "d"),
                            (interval.Hours, "h"),
                            (interval.Minutes, "m"),
                            (interval.Seconds, "s")
                        }
                        .Select((field, index) =>
                            field.Item1 >= 1
                                // index == 3 is second
                                ? $"{field.Item1.ToString().PadLeft(index == 3 ? 2 : 1, '0')}{field.Item2}"
                                : index == 3
                                    ? "0" + field.Item2
                                    : ""
                        )
                        .Where(output => output.Length > 0)
                        .ToArray();
                    return (thread.Id.ToString(), string.Join("", timeComponents));
                }
                catch
                {
                    return default;
                }
            }).Where(_ => _ != default).ToArray();

            var presentColumns = new[] { threads.Take(threads.Length / 2), threads.Skip(threads.Length / 2) }
                .Select(column =>
                {
                    var _ = new[] { ("Thread", "Uptime") }.Concat(column).ToArray();
                    var maxThreadIdLength = _.Select(row => row.Item1.Length).Max();
                    var maxUptimeLength = _.Select(row => row.Item2.Length).Max();
                    return _.Select(record =>
                    {
                        var (i1, i2) = record;
                        return (
                            i1
                                .PadLeft((maxThreadIdLength + i1.Length) / 2)
                                .PadRight(maxThreadIdLength),
                            i2
                                .PadLeft((maxUptimeLength + i2.Length) / 2)
                                .PadRight(maxUptimeLength)
                        );
                    }).ToArray();
                }).ToArray();

            return Reply(new LocalEmbed
            {
                Title = $"Process uptime : {ProcessUptime()}.",
                Description =
                    "```"
                    + string.Join('\n',
                        presentColumns[0].Zip(presentColumns[1],
                            (_1, _2) => $"{_1.Item1} | {_1.Item2} | {_2.Item1} | {_2.Item2}"
                        )
                    )
                    + "```",
                Footer = new LocalEmbedFooter
                {
                    Text = $"{RuntimeInformation.FrameworkDescription} ({RuntimeInformation.RuntimeIdentifier})"
                },
                Timestamp = DateTimeOffset.Now
            });
        }

        private static string ProcessUptime()
        {
            var interval = (DateTime.Now - Process.GetCurrentProcess().StartTime);
            // construct friendly time
            var timeComponents = new[]
                {
                    (interval.Days, "day"),
                    (interval.Hours, "hour"),
                    (interval.Minutes, "minute"),
                    (interval.Seconds, "second")
                }
                .Select(field =>
                    field.Item1 >= 1 ? $"**{field.Item1}** {(field.Item1 > 1 ? field.Item2.Pluralize() : field.Item2)}" : "")
                .Where(output => output.Length > 0)
                .ToArray();

            return timeComponents.Length == 1
                ? timeComponents[0]
                : string.Join(", ", new ArraySegment<string>(timeComponents, 0, timeComponents.Length - 1))
                  + $" and {timeComponents[^1]}";
        }
    }
}