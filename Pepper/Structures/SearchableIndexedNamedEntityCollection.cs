using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FuzzySharp;

namespace Pepper.Structures
{
    public class NamedKeyedEntity<TKey, TValue>
    {
        public readonly TValue Value;
        public readonly string Name;
        public readonly string[] Aliases;
        public readonly TKey Key;
        public NamedKeyedEntity(TKey key, TValue value, string name, IEnumerable<string> aliases)
        {
            Value = value;
            Name = name;
            Aliases = aliases.ToArray();
            Key = key;
        }
    }

    public class NamedKeyedEntitySearchResult<TKey, TValue> : NamedKeyedEntity<TKey, TValue>
    {
        public NamedKeyedEntitySearchResult(NamedKeyedEntity<TKey, TValue> keyedEntity, double score) 
            : base(keyedEntity.Key, keyedEntity.Value, keyedEntity.Name, keyedEntity.Aliases) => Score = score;
        public readonly double Score;
    }

    public class SearchableKeyedNamedEntityCollection<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> where TKey : notnull
    {
        private readonly NamedKeyedEntity<TKey, TValue>[] collection;
        private readonly Lazy<ImmutableDictionary<TKey, TValue>> lookupLazy;
        private ImmutableDictionary<TKey, TValue> Collection => lookupLazy.Value;
        public TValue this[TKey key] => lookupLazy.Value[key];
        public bool ContainsKey(TKey key) => lookupLazy.Value.ContainsKey(key);
        // public int Count => Collection.Count;

        public SearchableKeyedNamedEntityCollection(IEnumerable<NamedKeyedEntity<TKey, TValue>> collection)
        {
            this.collection = collection.ToArray();
            lookupLazy = new Lazy<ImmutableDictionary<TKey, TValue>>(
                () =>
                    this.collection
                        .GroupBy(entity => entity.Key)
                        .ToImmutableDictionary(
                            entity => entity.Key,
                            group => group.First().Value
                        )
            );
        }

        public NamedKeyedEntitySearchResult<TKey, TValue>[] FuzzySearch(string query, double nameWeight = 1f, double aliasWeight = 1.5f)
        {
            var entries = collection.Select(entry =>
            {
                var weightedNameSimilarity = Fuzz.WeightedRatio(entry.Name, query) * nameWeight;
                var weightedAliasSimilarities = entry.Aliases
                    .Select(alias => Fuzz.WeightedRatio(alias, query) * aliasWeight).ToList();
                var score = (weightedNameSimilarity + weightedAliasSimilarities.Sum()) /
                            (weightedAliasSimilarities.Count + 1);
                return new NamedKeyedEntitySearchResult<TKey, TValue>(entry, score);
            });

            var result = entries.ToList();
            result.Sort((r1, r2) => r2.Score.CompareTo(r1.Score));
            return result.ToArray();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Collection.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}