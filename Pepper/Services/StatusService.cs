using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Hosting;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Commons.Structures;

namespace Pepper.Services
{
    public interface IStatusProvider
    {
        public string GetCurrentStatus();
    }

    public class StatusService : Service
    {
        private Task SetStatus(string status, CancellationToken stoppingToken = default)
            => Client.SetPresenceAsync(new LocalActivity(status, ActivityType.Playing), stoppingToken);

        private class UptimeStatusProvider : IStatusProvider
        {
            public string GetCurrentStatus()
                => $"Uptime : {(DateTime.Now - Process.GetCurrentProcess().StartTime).Humanize(countEmptyUnits: true, precision: 3)}";
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Client.WaitUntilReadyAsync(stoppingToken);

            var services = Bot.Services.GetServices<DiscordClientService>().OfType<IStatusProvider>().ToList();
            services.Add(new UptimeStatusProvider());
            var index = 0;

            while (!stoppingToken.IsCancellationRequested)
            {
                await SetStatus(services[index].GetCurrentStatus(), stoppingToken);
                index = (index + 1) % services.Count;
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
        }
    }
}