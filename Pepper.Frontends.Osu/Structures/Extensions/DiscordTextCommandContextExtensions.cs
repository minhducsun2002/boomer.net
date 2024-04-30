using Disqord.Bot.Commands.Text;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Frontends.Osu.Services;
using Pepper.Frontends.Osu.Utils;

namespace Pepper.Commons.Extensions
{
    public static class DiscordTextCommandContextExtensions
    {
        public static async ValueTask<int?> GetBeatmapIdFromContext(this IDiscordTextCommandContext context)
        {
            var message = await context.GetReferencedMessage();
            if (message != null)
            {
                var embed = message.Embeds[0];
                if (embed?.Url != null)
                {
                    if (URLParser.CheckMapUrl(embed.Url, out _, out var id, out _) && id != null)
                    {
                        return id.Value;
                    }
                }

                if (URLParser.CheckMapUrl(message.Content, out _, out var id1, out _) && id1 != null)
                {
                    return id1.Value;
                }
            }


            var service = context.Services.GetService<BeatmapContextProviderService>();
            return service?.GetBeatmap(context.ChannelId.ToString());
        }
    }
}