using System.Diagnostics;
using System.Reflection;
using System.Threading.RateLimiting;
using Disqord;
using Disqord.Bot;
using Disqord.Rest;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.RateLimiting;

namespace Pepper.Commons.Structures
{
    public partial class Bot : DiscordBot
    {
        public static readonly string VersionHash = typeof(Bot)
            .Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(a => a.Key == "GitHash")?.Value ?? "unknown";

        public Bot(
            IOptions<DiscordBotConfiguration> options,
            ILogger<Bot> logger,
            IServiceProvider services,
            DiscordClient client
        ) : base(options, logger, services, client)
        {
            RegisterHealthCheck();
        }

        private void RegisterHealthCheck()
        {
            var rateLimiter = new ResiliencePipelineBuilder()
                .AddRateLimiter(new SlidingWindowRateLimiter(
                    new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromSeconds(15),
                        SegmentsPerWindow = 2
                    }
                )).Build();

            MessageReceived += async (_, args) =>
            {
                if (args.Message is not IUserMessage message)
                {
                    return;
                }

                if (!message.Author.IsBot)
                {
                    return;
                }

                var msg = args.Message;
                var currentUser = CurrentUser.Id;
                var content = message.Content;
                var template = "<@" + currentUser + "> " + "check";

                if (content != template)
                {
                    return;
                }

                try
                {
                    await rateLimiter.ExecuteAsync(async token =>
                    {
                        var reply = "<@" + msg.Author.Id + "> " + "check";
                        await this.SendMessageAsync(
                            message.ChannelId, new LocalMessage { Content = reply }.WithReply(
                                msg.Id,
                                msg.ChannelId,
                                msg.GuildId
                            ), cancellationToken: token);
                        return 1;
                    });
                }
                catch (RateLimiterRejectedException)
                {
                    // ignore
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception occurred replying to healthcheck: {0}", ex);
                }
            };
        }
    }
}
