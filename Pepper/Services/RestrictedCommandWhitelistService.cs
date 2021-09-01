using System.Collections.Immutable;
using System.Linq;
using BitFaster.Caching.Lru;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Pepper.Structures;
using Pepper.Structures.External.MongoDB;

namespace Pepper.Services
{
    public class Record : Document
    {
        [BsonElement("commandId")] public string CommandIdentifier = "";
        [BsonElement("guildId")] public string GuildId = "";
    }
    
    public class RestrictedCommandWhitelistService : Service
    {
        private readonly IMongoCollection<Record> collection;
        private readonly FastConcurrentLru<string, ImmutableHashSet<string>> cache = new(1000);
        public RestrictedCommandWhitelistService(IConfiguration config)
        {
            var value = config.GetSection("command:whitelist").Get<string[]>();
            var uri = value[0];
            MongoClient client = new(uri);
            collection = client.GetDatabase(value[1]).GetCollection<Record>(value[2]);
        }

        public ImmutableHashSet<string> GetAllowedGuilds(string commandIdentifier)
        {
            if (cache.TryGet(commandIdentifier, out var guilds)) return guilds;
            
            var result = collection.FindSync(record => record.CommandIdentifier == commandIdentifier)
                .ToList()
                .Select(record => record.GuildId)
                .ToImmutableHashSet();
            
            if (result.Count != 0) cache.AddOrUpdate(commandIdentifier, result);
            
            return result;
        }
    }
}