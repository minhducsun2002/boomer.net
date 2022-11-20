using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Extensions.Interactivity;
using Disqord.Rest;
using Humanizer;
using Humanizer.Localisation;
using Qmmands;
using Qmmands.Text;

namespace Pepper.Commands.General
{
    public class Ping : GeneralCommand
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(15);

        [TextCommand("ping")]
        [Description("Pong!")]
        public async Task<IDiscordCommandResult?> Exec()
        {
            var nonce = Context.Message.Id.ToString();
            var stopwatch = new Stopwatch();
            var waitTask = Bot.WaitForMessageAsync(
                Context.ChannelId,
                msgEvent =>
                    msgEvent.AuthorId == Bot.CurrentUser!.Id
                    && msgEvent.ChannelId == Context.ChannelId
                    && msgEvent.Message is IUserMessage message
                    && message.Nonce == nonce,
                Timeout
            );

            stopwatch.Start();
            var msg = await Response(
                new LocalMessage
                {
                    Reference = new LocalMessageReference
                    {
                        MessageId = Context.Message.Id
                    },
                    Nonce = nonce,
                    Content = "One second..."
                }
            );

            var result = await waitTask;
            stopwatch.Stop();
            if (result == null)
            {
                return Reply($"Didn't get the message back in time. The timeout was {Timeout.Humanize()}.");
            }

            await msg.ModifyAsync(
                m => m.Content = $"Pong! Roundtrip time was {stopwatch.Elapsed.Humanize(2, maxUnit: TimeUnit.Second)}."
            );
            return null;
        }
    }
}