using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Pepper.Services.FGO
{
    public class TraitService : NamingService
    {
        private readonly string url;
        private readonly ILogger log = Log.Logger.ForContext<TraitService>();
        private ConcurrentDictionary<long, string> traits = new();
        
        public TraitService(IConfiguration config)
        {
            var value = config.GetSection("fgo:traits:csv").Get<string[]>();
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