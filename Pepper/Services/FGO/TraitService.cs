using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Pepper.FuzzySearch;
using Pepper.Structures.External.FGO;
using Serilog;

namespace Pepper.Services.FGO
{
    public class TraitService : NamingService, ITraitNameProvider, IStatusProvider
    {
        private readonly string url;
        private readonly ILogger log = Log.Logger.ForContext<TraitService>();
        public ConcurrentDictionary<int, string> Traits = new();
        public Fuse<KeyValuePair<int, string>> FuzzySearch;

        private readonly MasterDataService masterDataService;
        
        public TraitService(IConfiguration config, MasterDataService masterDataService)
        {
            var value = config.GetSection("fgo:traits:csv").Get<string[]>();
            url = value[0];
            this.masterDataService = masterDataService;
        }

        public string GetTrait(int traitId, bool fallbackToEmpty = false)
        {
            string reverse = "";
            if (traitId < 0)
            {
                traitId = -traitId;
                reverse = "not-";
            }
            
            if (Traits.TryGetValue(traitId, out var traitName))
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
        
        public delegate void DataLoadedCallback(ConcurrentDictionary<int, string> traits);
        public event DataLoadedCallback? DataLoaded;
        
        public async Task<Dictionary<int, string>> Load()
        {
            var data = await GetCsv(url);
            var temporaryTraitMapping = data
                .Where(line => line.Count >= 2 && int.TryParse(line[0], out _))
                .Where(entry => !string.IsNullOrWhiteSpace(entry[1]))
                .Where(entry => int.Parse(entry[0]) != 0)
                .ToDictionary(
                    entry => int.Parse(entry[0]),
                    entry => entry[1]
                );
            Traits = new ConcurrentDictionary<int, string>(temporaryTraitMapping);
            FuzzySearch = new Fuse<KeyValuePair<int, string>>(
                Traits,
                false,
                new StringFuseField<KeyValuePair<int, string>>(kv => kv.Value)
            );
            log.Information($"Processed {Traits.Count} entries.");
            DataLoaded?.Invoke(Traits);
            return temporaryTraitMapping;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await Load();
            await base.StartAsync(cancellationToken);
        }

        public string GetCurrentStatus() => $"Cached {Traits.Count} traits";
    }
}