using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Structures;
using Serilog;

namespace Pepper.Services.FGO
{
    public class ServantNaming
    {
        public string Name = "";
        public string[] Aliases = Array.Empty<string>();
    }
    
    public class ServantNamingService : NamingService
    {
        private readonly string url;
        private readonly ILogger log = Log.Logger.ForContext<ServantNamingService>();
        public ConcurrentDictionary<long, ServantNaming> Namings { get; private set; } = new();

        public ServantNamingService(IConfiguration config)
        {
            var value = config.GetSection("fgo:aliases:csv").Get<string[]>();
            url = value[0];
        }

        public async Task<Dictionary<long, ServantNaming>> Load()
        {
            var data = await GetCsv(url);
            var naming = data
                .Select(line => line.Where(cell => !string.IsNullOrWhiteSpace(cell)).ToList())
                // collectionNo,servantId,servantOverwrite,servantAlias1,servantAlias2,...
                .Where(line => line.Count >= 3 && long.TryParse(line[0], out _) && long.TryParse(line[1], out _))
                .ToDictionary(
                    entry => long.Parse(entry[1]),
                    entry => new ServantNaming {Aliases = entry.Skip(3).ToArray(), Name = entry[2]}
                );
            Namings = new ConcurrentDictionary<long, ServantNaming>(naming);
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