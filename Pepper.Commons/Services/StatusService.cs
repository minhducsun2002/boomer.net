using Disqord;
using Disqord.Gateway;
using Disqord.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Commons.Interfaces;
using Pepper.Commons.Structures;

namespace Pepper.Commons.Services
{
    public class StatusService : Service
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Client.WaitUntilReadyAsync(stoppingToken);

            var services = Bot.Services.GetServices<DiscordClientService>()
                .OfType<IStatusProvidingService>()
                .ToList();

            if (services.Count == 0)
            {
                return;
            }
            var index = 0;

            while (!stoppingToken.IsCancellationRequested)
            {
                var status = services[index].GetCurrentStatus();
                if (!string.IsNullOrWhiteSpace(status))
                {
                    await Client.SetPresenceAsync(new LocalActivity(status, ActivityType.Playing), stoppingToken);
                    await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
                }
                index = (index + 1) % services.Count;
            }
        }
    }
}