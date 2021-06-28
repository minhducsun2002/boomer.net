using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Pepper.Structures;
using Pepper.Structures.External.FGO;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Services.FGO
{
    public enum Region
    {
        JP = 1 << 1,
        NA = 1 << 2
    }

    public class MasterDataService : Service
    {
        public Dictionary<Region, MasterDataMongoDBConnection> Connections = new();
        public Dictionary<Region, MongoClient> Clients { get; } = new();

        private static readonly Dictionary<string, Type> MasterDataEntityTypes = Pepper.AssemblyTypes
            .Where(type => typeof(MasterDataEntity).IsAssignableFrom(type) && !type.IsAbstract).ToDictionary(
                type => type.Name,
                type => type
            );

        public MasterDataService(IServiceProvider services)
        {
            var config = services.GetRequiredService<Configuration>();
            foreach (var regionCode in Enum.GetNames<Region>())
            {
                var cfg = config[$"database:fgo:master:{regionCode}"];
                var region = Enum.Parse<Region>(regionCode);
                Clients[region] = new MongoClient(cfg[0]);
                var db = Clients[region].GetDatabase(cfg[1]);
                
                // TODO : switch all of these to use code generation
                var connectionObject = new MasterDataMongoDBConnection();
                foreach (var field in connectionObject.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
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
            }
        }
        
        public override Task Initialize()
        {
            ConventionRegistry.Register(
                "IgnoreExtraElements",
                new ConventionPack { new IgnoreExtraElementsConvention(true) },
                type => true
            );
            return base.Initialize();
        }
    }
}