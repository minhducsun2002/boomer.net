using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Disqord.Bot;
using FuzzySharp;
using Microsoft.Extensions.DependencyInjection;
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

    public class ServantSearchRecord : ServantNaming
    {
        public int ServantId;
        public double Score;
        public int Bucket;
    }
    
    public class ServantIdentityTypeParser : DiscordTypeParser<ServantIdentity>
    {
        public static readonly ServantIdentityTypeParser Instance = new();

        public ServantSearchRecord[] Search(string query, IServiceProvider serviceProvider)
            => Search(
                query,
                serviceProvider.GetRequiredService<ServantSearchService>(),
                serviceProvider.GetRequiredService<ServantNamingService>().Namings
            );
        
        private static ServantSearchRecord[] Search(
            string query, 
            ServantSearchService servantSearchService,
            IDictionary<int, ServantNaming> servantNamings)
        {
            // servant => token occurence count
            var tokenSearchResult = TokenSearch(query, servantSearchService);

            var scores = servantNamings.Select(entry =>
            {
                const double nameWeight = 1.0f;
                const double aliasWeight = 1.5f;

                var (servantId, naming) = entry;
                var weightedNameSimilarity = Fuzz.WeightedRatio(naming.Name, query) * nameWeight;
                var weightedAliasSimilarities =
                    naming.Aliases.Select(alias => Fuzz.WeightedRatio(alias, query) * aliasWeight).ToList();
                var score = (weightedNameSimilarity + weightedAliasSimilarities.Sum()) /
                            (weightedAliasSimilarities.Count + 1);
                return new ServantSearchRecord
                {
                    ServantId = servantId,
                    Name = naming.Name,
                    Aliases = naming.Aliases,
                    Score = score,
                    Bucket = tokenSearchResult.TryGetValue(servantId, out var value) ? value : 0
                };
            });
            
            List<ServantSearchRecord> match = new(), mismatch = new();
            foreach (var record in scores)
                (tokenSearchResult.ContainsKey(record.ServantId) ? match : mismatch).Add(record);
            
            match.Sort((r1, r2) =>
            {
                if (tokenSearchResult[r1.ServantId] != tokenSearchResult[r2.ServantId])
                    return tokenSearchResult[r2.ServantId] - tokenSearchResult[r1.ServantId];

                return r2.Score.CompareTo(r1.Score);
            });
            mismatch.Sort((r1, r2) => r1.Score.CompareTo(r2.Score));;
            
            return match.Concat(mismatch).ToArray();
        }
        
        public override ValueTask<TypeParserResult<ServantIdentity>> ParseAsync(Parameter parameter, string value, DiscordCommandContext context)
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