using System;
using System.Collections.Generic;
using System.Linq;

namespace Pepper.FuzzySearch
{
    public class FuseSearchResult<T>
    {
        public readonly T Element;
        public readonly double Score;

        public FuseSearchResult(T element, double score)
        {
            Element = element;
            Score = score;
        }
    }
    
    public class Fuse<T>
    {
        private FuseIndex<T> index;
        private bool ignoreFieldNorm;
        
        public Fuse(FuseIndex<T> index) => this.index = index;
        public Fuse(IEnumerable<T> elements, bool ignoreFieldNorm = false, params FuseField<T>[] keys)
        {
            this.ignoreFieldNorm = ignoreFieldNorm;
            index = new FuseIndex<T>(keys);
            index.Create(elements);
        }

        public FuseSearchResult<T>[] Search(string query)
        {
            var searcher = new BitapSearch(query);
            var records = index.index;
            List<(T, IEnumerable<Match>, double)> results = new();
            foreach (var value in records.Values)
            {
                var matches = value.SubRecords
                    .Select(kv => FindMatches(kv.Key, kv.Value, searcher))
                    .Where(match => match != null)
                    .Select(match => (Match) match)
                    .ToList();
                
                // computeScore
                var totalScore = 1D;
                foreach (var match in matches)
                {
                    double weight = match.Key.Weight,
                        score = match.Score,
                        norm = match.Norm;
                    totalScore *= Math.Pow(
                        score == 0 && (weight != 0) ? double.Epsilon : score,
                        (weight != 0 ? weight : 1) * (ignoreFieldNorm ? 1 : norm)
                    );
                }

                var result = (value.Element, matches, totalScore);
                results.Add(result);
            }
            
            // lower score implies more accurate match
            return results
                .OrderBy(v => v.Item3)
                .Select(score => new FuseSearchResult<T>(score.Item1, score.Item3))
                .ToArray();
        }

        private struct Match
        {
            public double Score;
            public FuseField<T> Key;
            public string Text;
            public double Norm;
            public Range[] Indices;
        }

        private static Match? FindMatches(FuseField<T> key, (string, double) valueTuple, BitapSearch searcher)
        {
            var (text, norm) = valueTuple;
            var (isMatch, score, indices) = searcher.SearchIn(text);
            if (isMatch)
            {
                return new Match
                {
                    Score = score,
                    Key = key,
                    Text = text,
                    Norm = norm,
                    Indices = indices
                };
            }
            return null;
        }
    }
}