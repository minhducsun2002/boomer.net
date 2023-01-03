using System.Diagnostics;
using Humanizer;
using Pepper.Commons.Interfaces;
using Pepper.Commons.Structures;

namespace Pepper.Commons.Services
{
    public class UptimeService : Service, IStatusProvidingService
    {
        public string GetCurrentStatus() =>
            $"Uptime : {(DateTime.Now - Process.GetCurrentProcess().StartTime).Humanize(countEmptyUnits: true, precision: 3)}";
    }
}