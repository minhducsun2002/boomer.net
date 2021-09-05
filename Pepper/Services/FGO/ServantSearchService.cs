using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Pepper.Structures;
using Pepper.Structures.External.MongoDB;
using Serilog;

namespace Pepper.Services.FGO
{
    public class AliasEntry : Document
    {
        [BsonElement("collectionNo")] public int CollectionNo;
        [BsonElement("alias")] public string Alias = string.Empty;
        [BsonElement("addedAt")] public DateTimeOffset AddedAt = DateTimeOffset.MinValue;
        [BsonElement("creator")] public string CreatorId = string.Empty;
    }
    
    public class ServantSearchService : Service
    {
        private readonly ServantNamingService servantNamingService;
        private readonly MongoClient mongoClient;
        private readonly MasterDataService masterDataService;
        private readonly string dbName, collectionName;
        public Dictionary<string, HashSet<int>> TokenTable = new();
        public Dictionary<int, HashSet<int>> ServantTraits = new(); 
        private readonly ILogger log = Log.Logger.ForContext<ServantSearchService>();

        public bool TraitLoaded = false;

        public ServantSearchService(ServantNamingService namingService, MasterDataService masterDataService, IConfiguration config)
        {
            servantNamingService = namingService;
            this.masterDataService = masterDataService;
            var value = config.GetSection("database:fgo:aliases").Get<string[]>();

            mongoClient = new MongoClient(value[0]);
            dbName = value[1];
            collectionName = value[2];
            namingService.DataLoaded += ReloadTokenizationTable;
        }

        public List<AliasEntry> GetAlias(string query)
        {
            return mongoClient.GetDatabase(dbName).GetCollection<AliasEntry>(collectionName)
                .FindSync(alias => alias.Alias == query.ToLowerInvariant())
                .ToList();
        }

        private void ReloadTokenizationTable(IEnumerable<KeyValuePair<int, ServantNaming>> namings)
        {
            var @out = new Dictionary<string, HashSet<int>>();
            foreach (var (servantId, servantNaming) in namings)
            {
                var tokens = new List<string> { servantNaming.Name };
                tokens.AddRange(servantNaming.Aliases);
                var tokenized = tokens
                    .Select(alias => alias.Replace(")", "").Replace("(", ""))
                    .Select(alias => alias.ToLowerInvariant())
                    .SelectMany(alias => alias.Split(" ").Where(token => !string.IsNullOrWhiteSpace(token)));

                foreach (var token in tokenized)
                    if (!@out.TryAdd(token, new HashSet<int> { servantId }))
                        @out[token].Add(servantId);
            }
            
            TokenTable = @out;
            log.Information("Servant aliases tokenization complete.");
        }

        private void LoadServantTraits()
        {
            ServantTraits = masterDataService.Connections[Region.JP]
                .GetAllServantEntities()
                .ToDictionary(
                    entity => entity.ID,
                    entity => new HashSet<int>(entity.Traits)
                );
            
            log.Information($"Loaded trait table for {ServantTraits.Count} servants.");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            LoadServantTraits();
            TraitLoaded = true;
            return base.ExecuteAsync(stoppingToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            servantNamingService.DataLoaded -= ReloadTokenizationTable;
            return base.StopAsync(cancellationToken);
        }
    }
}