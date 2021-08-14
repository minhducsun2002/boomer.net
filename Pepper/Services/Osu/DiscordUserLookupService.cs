using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Disqord.Bot.Hosting;
using LazyCache;
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
        private readonly IAppCache usernameCache = new CachingService();
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
            usernameCache.Add(uid, results?.OsuUsername);
            return results;
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
            return await usernameCache.GetOrAddAsync(userId, UserGetter);
        }
    }
}