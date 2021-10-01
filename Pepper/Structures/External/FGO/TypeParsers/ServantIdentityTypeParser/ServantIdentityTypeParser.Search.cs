using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Services.FGO;

namespace Pepper.Structures.External.FGO.TypeParsers
{
    public partial class ServantIdentityTypeParser
    {
        public ServantSearchRecord[] Search(string query, IServiceProvider serviceProvider)
            => Search(
                query,
                serviceProvider.GetRequiredService<ServantSearchService>(),
                serviceProvider.GetRequiredService<ServantNamingService>().Namings
            );
        
        private static ServantSearchRecord[] Search(
            string query, 
            ServantSearchService servantSearchService,
            SearchableKeyedNamedEntityCollection<int, ServantNaming> servantNamings)
        {
            // servant => token occurence count
            var tokenSearchResult = TokenSearch(query, servantSearchService);

            var scores = servantNamings.FuzzySearch(query)
                .Select(entry => new ServantSearchRecord
                {
                    ServantId = entry.Element.Key,
                    Name = entry.Element.Name,
                    Aliases = entry.Element.Aliases,
                    Score = entry.Score,
                    Bucket = tokenSearchResult.TryGetValue(entry.Element.Key, out var value) ? value : 0 
                });

            List<ServantSearchRecord> match = new(), mismatch = new();
            foreach (var record in scores)
                (tokenSearchResult.ContainsKey(record.ServantId) ? match : mismatch).Add(record);
            
            match.Sort((r1, r2) =>
            {
                if (r1.Bucket != r2.Bucket) return r2.Bucket - r1.Bucket;

                return r1.Score.CompareTo(r2.Score);
            });
            mismatch.Sort((r1, r2) => r2.Score.CompareTo(r1.Score));;
            
            return match.Concat(mismatch).ToArray();
        }
    }
}