using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FuzzySharp;
using Pepper.FuzzySearch;

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
        private Fuse<NamedKeyedEntity<TKey, TValue>> fuse;

        public TValue this[TKey key] => lookupLazy.Value[key];
        public bool ContainsKey(TKey key) => lookupLazy.Value.ContainsKey(key);
        // public int Count => Collection.Count;

        public SearchableKeyedNamedEntityCollection(IEnumerable<NamedKeyedEntity<TKey, TValue>> collection, double nameWeight = 1D, double aliasWeight = 1.5D)
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
            fuse = new Fuse<NamedKeyedEntity<TKey, TValue>>(
                this.collection,
                false,
                new StringFuseField<NamedKeyedEntity<TKey, TValue>>(entity => entity.Name, nameWeight),
                new ArrayFuseField<NamedKeyedEntity<TKey, TValue>>(entity => entity.Aliases, aliasWeight)
            );
        }

        public delegate int Scorer(string input1, string input2);
        
        public FuseSearchResult<NamedKeyedEntity<TKey, TValue>>[] FuzzySearch(string query)
        {
            var results = fuse.Search(query);
            return results;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Collection.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}