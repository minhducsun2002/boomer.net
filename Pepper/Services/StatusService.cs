using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Humanizer;
using Pepper.Structures;

namespace Pepper.Services
{
    public class StatusService : Service
    {
        private Task SetStatus(CancellationToken stoppingToken = default)
        {
            var interval = DateTime.Now - Process.GetCurrentProcess().StartTime;
            
            return Client.SetPresenceAsync(
                new LocalActivity($"Uptime : {interval.Humanize(countEmptyUnits: true, precision: 3)}", ActivityType.Playing),
                stoppingToken
            );
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Client.WaitUntilReadyAsync(stoppingToken);
            await SetStatus(stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
                // TODO : Queries for all services implementing IStatusProvider and use information from there instead
                await SetStatus(stoppingToken);
            }
        }
    }
}