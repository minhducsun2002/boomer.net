using System;
using System.IO;
using System.Threading.Tasks;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;

namespace Pepper.Logging.Serilog.Sinks.Discord
{
    /// <summary>
    /// Serilog sink implementation for Discord Webhooks.
    /// </summary>
    public class DiscordSink : ILogEventSink, IDisposable, IAsyncDisposable
    {

        private readonly DiscordWebhookMessenger messenger;
        private readonly LogEventLevel minimumLevel;
        private readonly LoggingLevelSwitch? levelSwitch;
        private readonly IFormatProvider? formatProvider;
        private readonly ITextFormatter? textFormatter;
        private readonly bool disposeMessenger;

        public DiscordSink(DiscordWebhookMessenger messenger,
            LogEventLevel minimumLevel, LoggingLevelSwitch? levelSwitch,
            IFormatProvider? formatProvider,
            ITextFormatter? textFormatter,
            bool disposeMessenger)
        {
            this.messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            this.minimumLevel = minimumLevel;
            this.levelSwitch = levelSwitch;
            this.formatProvider = formatProvider;
            this.disposeMessenger = disposeMessenger;
            this.textFormatter = textFormatter;
        }

        /// <inheritdoc />
        public virtual void Emit(LogEvent logEvent)
        {
            if (logEvent.Level < minimumLevel)
            {
                return;
            }

            if (levelSwitch != null && logEvent.Level < levelSwitch.MinimumLevel)
            {
                return;
            }

            var message = logEvent.RenderMessage(formatProvider);
            if (textFormatter != null)
            {
                var writer = new StringWriter();
                textFormatter.Format(logEvent, writer);
                message = writer.ToString();
            }

            if (message.Length >= 1900)
            {
                message = message[..1900];
            }

            messenger.PushMessage(message);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (!disposeMessenger)
            {
                return;
            }

            try
            {
                // Give underlying messenger some time to drain the queue.
                messenger.ShutdownAsync().Wait(15 * 1000);
            }
            catch (Exception)
            {
                // According to an old convention, we shouldn't throw error in Dispose.
            }

            messenger.Dispose();
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            return messenger.DisposeAsync();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Provides extension methods for Discord logging sinks.
    /// </summary>
    public static class DiscordSinkExtensions
    {

        public static LoggerConfiguration Discord(this LoggerSinkConfiguration configuration,
            DiscordWebhookMessenger messenger,
            LogEventLevel minimumLevel = LogEventLevel.Verbose,
            LoggingLevelSwitch? levelSwitch = default,
            IFormatProvider? formatProvider = default,
            ITextFormatter? textFormatter = default,
            bool disposeMessenger = default)
        {
            return configuration.Sink(new DiscordSink(messenger, minimumLevel, levelSwitch, formatProvider, textFormatter, disposeMessenger));
        }

        public static LoggerConfiguration DiscordWebhook(this LoggerSinkConfiguration configuration,
            ulong webhookId, string webhookToken,
            LogEventLevel minimumLevel = LogEventLevel.Verbose,
            LoggingLevelSwitch? levelSwitch = default,
            IFormatProvider? formatProvider = default,
            ITextFormatter? formatter = default)
        {
            return configuration.Discord(new DiscordWebhookMessenger(webhookId, webhookToken),
                minimumLevel, levelSwitch, formatProvider, formatter, true);
        }

        public static LoggerConfiguration DiscordWebhook(this LoggerSinkConfiguration configuration,
            string webhookEndpointUrl,
            LogEventLevel minimumLevel = LogEventLevel.Verbose,
            LoggingLevelSwitch? levelSwitch = default,
            IFormatProvider? formatProvider = default,
            ITextFormatter? formatter = default)
        {
            return configuration.Discord(new DiscordWebhookMessenger(webhookEndpointUrl),
                minimumLevel, levelSwitch, formatProvider, formatter, true);
        }

    }
}