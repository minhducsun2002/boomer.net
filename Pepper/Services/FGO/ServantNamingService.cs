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
        public ConcurrentDictionary<int, ServantNaming> Namings { get; private set; } = new();
        public ConcurrentDictionary<string, int> ReverseNameLookupTable { get; private set; } = new();

        public ServantNamingService(IConfiguration config)
        {
            var value = config.GetSection("fgo:aliases:csv").Get<string[]>();
            url = value[0];
        }

        public async Task<Dictionary<int, ServantNaming>> Load()
        {
            var data = await GetCsv(url);
            var naming = data
                .Select(line => line.Where(cell => !string.IsNullOrWhiteSpace(cell)).ToList())
                // collectionNo,servantId,servantOverwrite,servantAlias1,servantAlias2,...
                .Where(line => line.Count >= 3 && int.TryParse(line[0], out _) && int.TryParse(line[1], out _))
                .ToDictionary(
                    entry => int.Parse(entry[1]),
                    entry => new ServantNaming {Aliases = entry.Skip(3).ToArray(), Name = entry[2]}
                );

            var reverseLookupTable = naming.SelectMany(pair =>
                {
                    var (servantId, naming) = pair;
                    var _ = new List<string> { naming.Name };
                    _.AddRange(naming.Aliases);
                    return _.Select(name => (name, servantId));
                })
                .GroupBy(pair => pair.name)
                .ToDictionary(
                    grouping => grouping.Key,
                    grouping => grouping.First().servantId
                );

            ReverseNameLookupTable = new ConcurrentDictionary<string, int>(reverseLookupTable);
            Namings = new ConcurrentDictionary<int, ServantNaming>(naming);
            
            
            log.Information($"Processed {Namings.Count} entries.");
            DataLoaded?.Invoke(Namings);
            return naming;
        }

        public delegate void DataLoadedCallback(ConcurrentDictionary<int, ServantNaming> namings);
        public event DataLoadedCallback? DataLoaded;

        public override async Task StartAsync(CancellationToken stoppingToken)
        {
            await Load();
            await base.ExecuteAsync(stoppingToken);
        }
    }
}