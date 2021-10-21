using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        public SearchableKeyedNamedEntityCollection<int, ServantNaming> Namings = new(Array.Empty<NamedKeyedEntity<int, ServantNaming>>());
        public ImmutableDictionary<int, int> ServantIdToCollectionNo { get; private set; } = ImmutableDictionary<int, int>.Empty;

        public ServantNamingService(IConfiguration config)
        {
            var value = config.GetSection("fgo:aliases:csv").Get<string[]>();
            url = value[0];
        }

        public async Task<SearchableKeyedNamedEntityCollection<int, ServantNaming>> Load()
        {
            var data = await GetCsv(url);
            var validLines = data
                .Select(line => line.Where(cell => !string.IsNullOrWhiteSpace(cell)).ToList())
                // collectionNo,servantId,servantOverwrite,servantAlias1,servantAlias2,...
                .Where(line => line.Count >= 3 && int.TryParse(line[0], out _) && int.TryParse(line[1], out _))
                .ToList();

            ServantIdToCollectionNo = validLines
                .ToImmutableDictionary(
                    entry => int.Parse(entry[1]),
                    entry => int.Parse(entry[0])
                );

            Namings = new SearchableKeyedNamedEntityCollection<int, ServantNaming>(
                validLines
                    .Select(entry => new NamedKeyedEntity<int, ServantNaming>(
                        int.Parse(entry[1]),
                        new ServantNaming { Aliases = entry.Skip(3).ToArray(), Name = entry[2] },
                        entry[2],
                        entry.Skip(3).ToArray()
                    ))
            );

            log.Information($"Processed {validLines.Count} entries.");
            DataLoaded?.Invoke(Namings);
            return Namings;
        }

        public delegate void DataLoadedCallback(IEnumerable<KeyValuePair<int, ServantNaming>> namings);
        public event DataLoadedCallback? DataLoaded;

        public override async Task StartAsync(CancellationToken stoppingToken)
        {
            await Load();
            await base.ExecuteAsync(stoppingToken);
        }
    }
}