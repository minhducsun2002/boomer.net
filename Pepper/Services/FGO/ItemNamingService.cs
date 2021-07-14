using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Structures;
using Serilog;

namespace Pepper.Services.FGO
{
    public class ItemNamingService : NamingService
    {
        public ConcurrentDictionary<long, string> Namings { get; set; } = new();
        private readonly string url;
        private readonly ILogger log = Log.Logger.ForContext<ItemNamingService>();
        
        public ItemNamingService(IConfiguration config)
        {
            var value = config.GetSection("fgo:items:csv").Get<string[]>();
            url = value[0];
        }
        
        public async Task<Dictionary<long, string>> Load()
        {
            var data = await GetCsv(url);
            var naming = data
                .Select(line => line.Where(cell => !string.IsNullOrWhiteSpace(cell)).ToList())
                .Where(line => line.Count >= 2 && long.TryParse(line[0], out _))
                .ToDictionary(entry => long.Parse(entry[0]), entry => entry[1]);
            Namings = new ConcurrentDictionary<long, string>(naming);
            log.Information($"Processed {Namings.Count} entries.");
            return naming;
        }

        public override async Task StartAsync(CancellationToken stoppingToken)
        {
            await Load();
            await base.ExecuteAsync(stoppingToken);
        }
    }
}