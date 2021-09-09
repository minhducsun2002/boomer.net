using System;
using System.Collections.Generic;
using MongoDB.Driver;
using Pepper.Services.FGO;

namespace Pepper.Structures.External.FGO
{
    public partial class ServantIdentityTypeParser
    {
        private readonly MongoClient mongoClient;
        private readonly string dbName, collectionName;
        
        public void AddAlias(string alias, int collectionNo, string creatorId)
            => mongoClient.GetDatabase(dbName).GetCollection<AliasEntry>(collectionName)
                .InsertOne(new AliasEntry
                {
                    CollectionNo = collectionNo,
                    Alias = alias.ToLowerInvariant(),
                    CreatorId = creatorId,
                    AddedAt = DateTimeOffset.Now
                });

        public List<AliasEntry> GetAlias(int collectionNo)
            => mongoClient.GetDatabase(dbName).GetCollection<AliasEntry>(collectionName)
                .FindSync(alias => alias.CollectionNo == collectionNo)
                .ToList();
        
        public List<AliasEntry> GetAlias(string query)
            => mongoClient.GetDatabase(dbName).GetCollection<AliasEntry>(collectionName)
                .FindSync(alias => alias.Alias == query.ToLowerInvariant())
                .ToList();
    }
}