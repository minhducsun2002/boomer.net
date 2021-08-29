using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Pepper.Structures.External.FGO.MasterData;
using Serilog;

namespace Pepper.Services.FGO
{
    public class TraitService : NamingService
    {
        private readonly string url;
        private readonly ILogger log = Log.Logger.ForContext<TraitService>();
        private ConcurrentDictionary<long, string> traits = new();
        private readonly MasterDataService masterDataService;
        
        public TraitService(IConfiguration config, MasterDataService masterDataService)
        {
            var value = config.GetSection("fgo:traits:csv").Get<string[]>();
            url = value[0];
            this.masterDataService = masterDataService;
        }

        public string GetTrait(long traitId, bool fallbackToEmpty = false)
        {
            string reverse = "";
            if (traitId < 0)
            {
                traitId = -traitId;
                reverse = "not-";
            }
            
            if (traits.TryGetValue(traitId, out var traitName))
                return reverse + traitName;
            
            // tries to resolve to servant names
            foreach (var region in masterDataService.Regions)
            {
                var connection = masterDataService.Connections[region];
                var result = connection.GetServantEntityById((int) traitId);
                if (result != default) return reverse + result.Name;
            }

            return (fallbackToEmpty ? "" : $"{reverse}{traitId}");
        }
        
        public async Task<Dictionary<long, string>> Load()
        {
            var data = await GetCsv(url);
            var temporaryTraitMapping = data
                .Where(line => line.Count >= 2 && long.TryParse(line[0], out _))
                .Where(entry => !string.IsNullOrWhiteSpace(entry[1]))
                .ToDictionary(
                    entry => long.Parse(entry[0]),
                    entry => entry[1]
                );
            traits = new ConcurrentDictionary<long, string>(temporaryTraitMapping);
            log.Information($"Processed {this.traits.Count} entries.");
            return temporaryTraitMapping;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await Load();
            await base.StartAsync(cancellationToken);
        }
    }
}