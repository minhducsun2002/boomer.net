using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Structures;
using Serilog;

namespace Pepper.Services.FGO
{
    public class TraitService : NamingService
    {
        private readonly string url;
        private readonly ILogger log = Log.Logger.ForContext<TraitService>();
        private ConcurrentDictionary<long, string> traits = new();
        
        public TraitService(IServiceProvider services)
        {
            var config = services.GetRequiredService<Configuration>();
            var value = config["fgo:traits:csv"];
            url = value[0];
        }

        public string GetTrait(long traitId, bool fallbackToEmpty = false)
        {
            return traits.TryGetValue(traitId, out var traitName)
                ? traitName
                : (fallbackToEmpty ? "" : $"{traitId}");
        }
        
        public async Task<Dictionary<long, string>> Load()
        {
            var data = await GetCsv(url);
            var traits = data
                .Where(line => line.Count >= 2 && long.TryParse(line[0], out _))
                .Where(entry => !string.IsNullOrWhiteSpace(entry[1]))
                .ToDictionary(
                    entry => long.Parse(entry[0]),
                    entry => entry[1]
                );
            this.traits = new ConcurrentDictionary<long, string>(traits);
            log.Information($"Processed {this.traits.Count} entries.");
            return traits;
        }
        
        public override async Task Initialize()
        {
            await Load();
        }
    }
}