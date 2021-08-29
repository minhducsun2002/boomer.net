using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Disqord.Bot;
using FgoExportedConstants;
using FuzzySharp;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Pepper.Services.FGO;
using Pepper.Structures.External.FGO.MasterData;
using Qmmands;

namespace Pepper.Structures.External.FGO
{
    public class ServantIdentity
    {
        public int ServantId;
        public static implicit operator int(ServantIdentity servant) => servant.ServantId;
    }
    
    public class ServantIdentityTypeParser : DiscordTypeParser<ServantIdentity>
    {
        public static readonly ServantIdentityTypeParser Instance = new();
        
        public override async ValueTask<TypeParserResult<ServantIdentity>> ParseAsync(Parameter parameter, string value, DiscordCommandContext context)
        {
            var masterDataService = context.Services.GetRequiredService<MasterDataService>();
            if (int.TryParse(value, out var numericIdentity))
            {
                var result = ResolveNumericIdentifier(numericIdentity, masterDataService);
                return result?.ID == null 
                    ? Failure($"Could not find a servant with collectionNo/ID {numericIdentity}.") 
                    : Success(new ServantIdentity { ServantId = result.ID });
            }

            var query = value.ToLowerInvariant();
            var servantSearchService = context.Services.GetRequiredService<ServantSearchService>();
            var alias = servantSearchService.GetAlias(query);
            if (alias.Count != 0)
            {
                var result = ResolveNumericIdentifier(alias[0].CollectionNo, masterDataService);
                return result?.ID == null 
                    ? Failure($"Could not find a servant with collectionNo {numericIdentity}.") 
                    : Success(new ServantIdentity { ServantId = result.ID });
            }
                
            var reverseLookupTable = context.Services.GetRequiredService<ServantNamingService>().ReverseNameLookupTable;
            // servant => count
            var tokenSearchResult = TokenSearch(query, servantSearchService);
            var res = Process.ExtractTop(
                query,
                reverseLookupTable.Keys,
                name => name.ToLowerInvariant(),
                limit: 100
            );
            
            // remap back
            var servantIds = res.Select(res => (reverseLookupTable[res.Value], res.Score));
            List<(int, int)> match = new(), mismatch = new();
            foreach (var record in servantIds)
                (tokenSearchResult.ContainsKey(record.Item1) ? match : mismatch).Add(record);
            
            match.Sort((r1, r2) =>
            {
                var (servantId1, score1) = r1;
                var (servantId2, score2) = r2;
                if (tokenSearchResult[servantId1] != tokenSearchResult[servantId2])
                    return tokenSearchResult[servantId2] - tokenSearchResult[servantId1];

                return score1 - score2;
            });

            return Success(new ServantIdentity { ServantId = match.Concat(mismatch).First().Item1 });
        }


        private static MstSvt? ResolveNumericIdentifier(int idOrCollectionNo, MasterDataService masterDataService)
        {
            var jp = masterDataService.Connections[Region.JP];
            return jp.GetMstSvtById(idOrCollectionNo) ?? jp.GetServantEntityByCollectionNo(idOrCollectionNo);
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