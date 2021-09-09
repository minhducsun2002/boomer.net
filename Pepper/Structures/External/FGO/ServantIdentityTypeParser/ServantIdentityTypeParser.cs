using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Disqord.Bot;
using FuzzySharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Pepper.Services.FGO;
using Pepper.Structures.External.FGO.Entities;
using Pepper.Structures.External.FGO.MasterData;
using Qmmands;


namespace Pepper.Structures.External.FGO
{
    public class ServantSearchRecord : ServantNaming
    {
        public int ServantId;
        public double Score;
        public int Bucket;
    }
    
    public partial class ServantIdentityTypeParser : DiscordTypeParser<ServantIdentity>
    {
        private readonly ServantNamingService servantNamingService;
        private readonly MasterDataService masterDataService;
        public ServantIdentityTypeParser(ServantNamingService namingService, MasterDataService masterDataService, IConfiguration config)
        {
            servantNamingService = namingService;
            this.masterDataService = masterDataService;
            var value = config.GetSection("database:fgo:aliases").Get<string[]>();

            mongoClient = new MongoClient(value[0]);
            dbName = value[1];
            collectionName = value[2];
        }

        public override ValueTask<TypeParserResult<ServantIdentity>> ParseAsync(Parameter parameter, string value, DiscordCommandContext context)
        {
            if (int.TryParse(value, out var numericIdentity))
            {
                var result = ResolveNumericIdentifier(numericIdentity, masterDataService);
                return result?.ID == null 
                    ? Failure($"Could not find a servant with collectionNo/ID {numericIdentity}.") 
                    : Success(new ServantIdentity { ServantId = result.ID });
            }

            var query = value.ToLowerInvariant();
            var servantSearchService = context.Services.GetRequiredService<ServantSearchService>();
            var alias = GetAlias(query);
            if (alias.Count != 0)
            {
                var result = ResolveNumericIdentifier(alias[0].CollectionNo, masterDataService);
                return result?.ID == null 
                    ? Failure($"Could not find a servant with collectionNo {numericIdentity}.") 
                    : Success(new ServantIdentity { ServantId = result.ID });
            }

            var search = Search(
                query,
                servantSearchService,
                context.Services.GetRequiredService<ServantNamingService>().Namings
            );

            return Success(new ServantIdentity { ServantId = search.First().ServantId });
        }


        private static MstSvt? ResolveNumericIdentifier(int idOrCollectionNo, MasterDataService masterDataService)
        {
            var jp = masterDataService.Connections[Region.JP];
            return jp.GetServantEntityById(idOrCollectionNo) ?? jp.GetServantEntityByCollectionNo(idOrCollectionNo);
        }
        
        private static readonly Regex Whitespaces = new(@"\s", RegexOptions.Compiled | RegexOptions.ECMAScript | RegexOptions.CultureInvariant);
        private static Dictionary<int, int> TokenSearch(string query, ServantSearchService searchService)
        {
            var split = Whitespaces.Replace(query, " ").Split(" ").Where(str => !string.IsNullOrWhiteSpace(str));
            var validTokens = split.Where(token => searchService.TokenTable.ContainsKey(token));
            var tokenRelatedServants = validTokens.Select(token => (token, searchService.TokenTable[token])).ToList();

            var servantOccurences = tokenRelatedServants
                .SelectMany(h => h.Item2.Select(servantId => (servantId, h.token)))
                .GroupBy(pair => pair.servantId)
                .ToDictionary(
                    group => group.Key,
                    group => group.Count()
                );

            return servantOccurences;
        }
    }
}