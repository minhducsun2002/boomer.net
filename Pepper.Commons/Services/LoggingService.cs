using Disqord;
using Disqord.Gateway;
using Microsoft.Extensions.Logging;
using Pepper.Commons.Interfaces;
using Pepper.Commons.Structures;

namespace Pepper.Commons.Services
{
    public class LoggingService : Service, IStatusProvidingService
    {
        private readonly ILogger<LoggingService> loggingService;
        public LoggingService(ILogger<LoggingService> loggingService) => this.loggingService = loggingService;
        public bool Enabled = false;

        protected override ValueTask OnInteractionReceived(InteractionReceivedEventArgs e)
        {
            if (!Enabled)
            {
                return ValueTask.CompletedTask;
            }

            if (e.Interaction is TransientComponentInteraction interaction)
            {
                loggingService.LogInformation(
                    "Interaction received : channel {0}, user {1}, custom ID \"{2}\", type {3}",
                    interaction.ChannelId,
                    $"{interaction.Author.Id} ({interaction.Author.Name}#{interaction.Author.Discriminator})",
                    interaction.CustomId,
                    interaction.ComponentType
                );
            }
            return ValueTask.CompletedTask;
        }

        public string GetCurrentStatus() => Enabled ? "Verbose logging is enabled" : "";
    }
}