using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Pepper.FuzzySearch
{
    internal class NormGenerator
    {
        private Dictionary<int, double> cache = new();
        private static readonly Regex Matcher = new("/[^ ]+/", RegexOptions.Compiled | RegexOptions.ECMAScript | RegexOptions.CultureInvariant);
        
        public double Get(string value)
        {
            var tokenCount = Matcher.Match(value).Length + 1;
            if (cache.TryGetValue(tokenCount, out var fieldNorm)) return fieldNorm;
            
            return cache[tokenCount] = 1F / Math.Sqrt(tokenCount);
        }
        public void Clear() => cache = new Dictionary<int, double>();
    }
}