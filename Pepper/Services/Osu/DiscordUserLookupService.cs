using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BAMCIS.ChunkExtensionMethod;
using Disqord.Bot.Hosting;
using LazyCache;
using LazyCache.Providers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Pepper.Structures;
using Pepper.Structures.External.MongoDB;
using Serilog;

namespace Pepper.Services.Osu
{
    public class DiscordOsuUsernameRecord
    {
        [BsonElement("discordUserId")] public string DiscordUserId;
        [BsonElement("osuUsername")] public string OsuUsername;
    }
    
    public class DiscordOsuUsernameLookupService : Service
    {
        private MongoClient client = null!;
        private readonly string collectionName;
        private readonly string databaseName;
        private readonly string serverUri;
        private readonly IAppCache usernameCache = new CachingService(new MemoryCacheProvider(new MemoryCache(new MemoryCacheOptions
        {
            SizeLimit = 500
        })));
        private readonly ILogger log = Log.Logger.ForContext<DiscordOsuUsernameLookupService>();

        private IMongoCollection<DiscordOsuUsernameRecord> Collection => client.GetDatabase(databaseName)
            .GetCollection<DiscordOsuUsernameRecord>(collectionName);

        public DiscordOsuUsernameLookupService(IConfiguration configuration)
        {
            var record = configuration.GetSection("database:osu:usernames").Get<string[]>();
            serverUri = record[0]; databaseName = record[1]; collectionName = record[2];
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            client = new MongoClient(serverUri);
            log.Debug($@"Connecting to osu! username server at {
                string.Join(", ", client.Settings.Servers.Select(s => s.Host + ":" + s.Port))
            }");
            return base.StartAsync(cancellationToken);
        }

        public DiscordOsuUsernameRecord? StoreUser(ulong discordUserId, string username)
        {
            var uid = discordUserId.ToString();
            var filter = Builders<DiscordOsuUsernameRecord>.Filter.Eq(record => record.DiscordUserId, uid);
            var results = Collection.FindOneAndReplace(filter, new DiscordOsuUsernameRecord
            {
                DiscordUserId = uid,
                OsuUsername = username,
            }, new FindOneAndReplaceOptions<DiscordOsuUsernameRecord>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            });
            usernameCache.Remove(uid);
            usernameCache.Add(uid, results?.OsuUsername, new MemoryCacheEntryOptions { Size = 1 });
            return results;
        }

        public async Task<Dictionary<ulong, string>> GetManyUsers(params ulong[] discordUserIds)
        {
            if (discordUserIds.Length == 0) return new Dictionary<ulong, string>();

            var output = new Dictionary<ulong, string>(); 
            var uncached = new List<ulong>();
            
            foreach (var uid in discordUserIds)
                if (usernameCache.TryGetValue<string>(uid.ToString(), out var username) && username != null)
                    output[uid] = (string) username;
                else uncached.Add(uid);

            var usernames = uncached
                .Where(uid => !output.ContainsKey(uid))
                .Chunk(100)
                .Select(chunk => Builders<DiscordOsuUsernameRecord>.Filter.Or(
                    chunk.Select(uid => Builders<DiscordOsuUsernameRecord>.Filter.Eq("discordUserId", uid.ToString()))
                ))
                .SelectMany(filterExpression => Collection.Find(filterExpression).ToList());

            foreach (var record in usernames) output[ulong.Parse(record.DiscordUserId)] = record.OsuUsername;
            foreach (var (uid, username) in output)
            {
                usernameCache.Remove(uid.ToString());
                usernameCache.Add(uid.ToString(), username, new MemoryCacheEntryOptions { Size = 1 });
            }                
            return output;
        }
        
        public async Task<string?> GetUser(ulong discordUserId)
        {
            var userId = discordUserId.ToString();
            async Task<string?> UserGetter()
            {
                var filter = Builders<DiscordOsuUsernameRecord>.Filter.Eq(record => record.DiscordUserId, userId);
                var results = Collection.Find(filter);
                var count = await results.CountDocumentsAsync();
                if (count == 0)
                {
                    Log.Debug($"Username not found for user ID \"{userId}\"");
                    return null;
                }

                var username = results.Limit(1).First().OsuUsername;
                Log.Debug($"Found username \"{username}\" bound to user ID \"{userId}\".");
                return username;
            }
            return await usernameCache.GetOrAddAsync(userId, UserGetter, new MemoryCacheEntryOptions { Size = 1 });
        }
    }
}