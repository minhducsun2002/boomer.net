using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Pepper.Structures;
using Serilog;

namespace Pepper.Services.FGO
{
    public class CraftEssenceNamingService : NamingService
    {
        private readonly string url;
        private readonly ILogger log = Log.Logger.ForContext<CraftEssenceNamingService>();
        public ImmutableDictionary<int, int> CEIdToCollectionNo { get; private set; } = ImmutableDictionary<int, int>.Empty;
        public SearchableKeyedNamedEntityCollection<int, string> Namings = new(Array.Empty<NamedKeyedEntity<int, string>>());

        public CraftEssenceNamingService(IConfiguration config)
        {
            var value = config.GetSection("fgo:ce_aliases:csv").Get<string[]>();
            url = value[0];
        }

        public async Task<SearchableKeyedNamedEntityCollection<int, string>> Load()
        {
            var data = await GetCsv(url);
            var validLines = data
                .Select(line => line.Where(cell => !string.IsNullOrWhiteSpace(cell)).ToList())
                // collectionNo,ceId,name
                .Where(line => line.Count >= 3 && int.TryParse(line[0], out _) && int.TryParse(line[1], out _))
                .ToList();

            CEIdToCollectionNo = validLines
                .ToImmutableDictionary(
                    entry => int.Parse(entry[1]),
                    entry => int.Parse(entry[0])
                );

            Namings = new SearchableKeyedNamedEntityCollection<int, string>(
                validLines
                    .Select(entry => new NamedKeyedEntity<int, string>(
                        int.Parse(entry[1]),
                        value: entry[2],
                        entry[2],
                        Array.Empty<string>()
                    ))
            );

            log.Information($"Processed {validLines.Count} entries.");
            DataLoaded?.Invoke(Namings);
            return Namings;
        }

        public delegate void DataLoadedCallback(IEnumerable<KeyValuePair<int, string>> namings);
        public event DataLoadedCallback? DataLoaded;

        public override async Task StartAsync(CancellationToken stoppingToken)
        {
            await Load();
            await base.ExecuteAsync(stoppingToken);
        }
    }
}