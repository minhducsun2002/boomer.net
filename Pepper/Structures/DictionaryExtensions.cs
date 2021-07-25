using System.Collections.Generic;

namespace Pepper.Structures
{
    public static class DictionaryExtensions
    {
        public static TValue Consume<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) where TKey : notnull
        {
            var value = dict[key];
            dict.Remove(key);
            return value;
        }
    }
}