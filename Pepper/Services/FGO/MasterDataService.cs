using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Pepper.Structures;
using Pepper.Structures.External.FGO;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Services.FGO
{
    public enum Region
    {
        NA = 1 << 1,
        JP = 1 << 0
    }

    public class MasterDataService : Service
    {
        public ConcurrentDictionary<Region, IMasterDataProvider> Connections = new();
        public ConcurrentDictionary<Region, MongoClient> Clients { get; } = new();
        public readonly List<Region> Regions;

        private static readonly Dictionary<string, Type> MasterDataEntityTypes = typeof(Pepper)
            .Assembly.GetTypes()
            .Where(type => typeof(MasterDataEntity).IsAssignableFrom(type) && !type.IsAbstract).ToDictionary(
                type => type.Name,
                type => type
            );

        public MasterDataService(IConfiguration config)
        {
            var regions = new List<Region>();
            foreach (var regionCode in Enum.GetNames<Region>())
            {
                var cfg = config.GetSection($"database:fgo:master:{regionCode}").Get<string[]>();
                var region = Enum.Parse<Region>(regionCode);
                Clients[region] = new MongoClient(cfg[0]);
                var db = Clients[region].GetDatabase(cfg[1]);
                
                // TODO : switch all of these to use code generation
                var connectionObject = new MasterDataMongoDBConnection();
                foreach (var field in connectionObject.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance))
                    if (MasterDataEntityTypes.ContainsKey(field.Name))
                    {
                        var method = db.GetType().GetMethod(nameof(db.GetCollection))!
                            .MakeGenericMethod(MasterDataEntityTypes[field.Name]);
                        
                        // call generic method GetCollection with proper entity type
                        var collectionName = char.ToLowerInvariant(field.Name[0]) + field.Name[1..];
                        
                        // and assign the return type back to the field
                        field.SetValue(connectionObject, method.Invoke(db, new object[] { collectionName, null! }));
                        
                        // equivalent :
                        // connectionObject.MstSvt = db.GetCollection<MstSvt>("mstSvt")
                    }

                Connections[region] = connectionObject;
                regions.Add(region);
            }

            Regions = regions;
        }
    }
}