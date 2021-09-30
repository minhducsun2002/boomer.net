using System;
using System.Collections.Generic;

namespace Pepper.FuzzySearch
{
    internal class BitapSearch
    {
        private string pattern;
        private List<(string, Dictionary<char, int>, int)> chunks;
        private const int MaxBits = 32;
        private bool ignoreLocation, isCaseSensitive;
        private int minMatchCharLength;

        public BitapSearch(
            string pattern, int minMatchCharLength = 1, bool isCaseSensitive = false,
            bool ignoreLocation = true)
        {
            this.pattern = isCaseSensitive ? pattern : pattern.ToLowerInvariant();
            this.ignoreLocation = ignoreLocation;
            this.minMatchCharLength = minMatchCharLength;
            this.isCaseSensitive = isCaseSensitive;

            chunks = new List<(string, Dictionary<char, int>, int)>();
            var len = this.pattern.Length;

            void AddChunk(string patternToAdd, int startIndex)
            {
                chunks.Add((patternToAdd, CreatePatternAlphabet(patternToAdd), startIndex));
            }

            if (len > MaxBits)
            {
                int i = 0, rem = len % MaxBits, end = len - rem;
                while (i < end)
                {
                    AddChunk(
                        this.pattern.Substring(i, Math.Min(i + MaxBits, this.pattern.Length - i)),
                        i
                    );
                    i += MaxBits;
                }

                if (rem != 0)
                {
                    var startIndex = len - MaxBits;
                    AddChunk(this.pattern[startIndex..], startIndex);
                }
            }
            else
            {
                AddChunk(this.pattern, 0);
            }
        }

        public (bool, double, Range[]) SearchIn(string text)
        {
            text = isCaseSensitive ? text : text.ToLowerInvariant();
            if (pattern == text)
            {
                return (true, 0, new [] { new Range(0, pattern.Length - 1) });
            }

            var hasMatches = false;
            double totalScore = 0;
            List<Range> allIndices = new();
            foreach (var (pattern, alphabet, startIndex) in chunks)
            {
                const int location = 0;
                
                var (isMatch, score, indices) = Search(
                    text, pattern, alphabet,
                    location: location + startIndex,
                    ignoreLocation: ignoreLocation,
                    minMatchCharLength: minMatchCharLength
                );

                if (isMatch)
                {
                    hasMatches = true;
                }

                totalScore += score;
                if (isMatch && indices.Length != 0) allIndices.AddRange(indices);
            }

            return (hasMatches, hasMatches ? totalScore / chunks.Count : 1, allIndices.ToArray());
        }

        private static (bool, double, Range[]) Search(
            string text, string pattern, Dictionary<char, int> alphabet,
            int location,
            bool ignoreLocation,
            int minMatchCharLength)
        {
            int patternLength = pattern.Length,
                textLength = text.Length,
                expectedLocation = Math.Max(0, Math.Min(location, textLength));
            var currentThreshold = 0.6D;
            var bestLocation = expectedLocation;
            const bool computeMatches = true;
            var matchMask = new int[textLength];

            var index = 0;
            while ((index = text.IndexOf(pattern, bestLocation, StringComparison.InvariantCulture)) > -1)
            {
                var score = BitapAlgorithm.ComputeScore(
                    pattern,
                    currentLocation: index,
                    expectedLocation: expectedLocation,
                    ignoreLocation: ignoreLocation
                );

                currentThreshold = Math.Min(score, currentThreshold);
                bestLocation = index + patternLength;
                if (computeMatches)
                {
                    var i = 0;
                    while (i < patternLength)
                    {
                        matchMask[index + i] = 1;
                        i += 1;
                    }
                }
            }

            // bestLocation = -1;
            var lastBitArr = Array.Empty<int>();
            double finalScore = 1;
            var binMax = patternLength + textLength;
            var mask = 1 << (patternLength - 1);
            for (var i = 0; i < patternLength; i += 1)
            {
                int binMin = 0, binMid = binMax;
                while (binMin < binMid)
                {
                    var localScore = BitapAlgorithm.ComputeScore(
                        pattern,
                        errors: i,
                        currentLocation: expectedLocation + binMid,
                        expectedLocation: expectedLocation,
                        ignoreLocation: ignoreLocation
                    );

                    if (localScore <= currentThreshold)
                        binMin = binMid;
                    else
                        binMax = binMid;

                    binMid = (int)Math.Floor((double)(binMax - binMin) / 2 + binMin);
                }

                binMax = binMid;

                var start = Math.Max(1, expectedLocation - binMid + 1);
                var finish = textLength;

                var bitArr = new int[finish + 2];
                bitArr[finish + 1] = (1 << i) - 1;
                for (var j = finish; j >= start; j -= 1)
                {
                    var currentLocation = j - 1;
                    if (!alphabet.TryGetValue(text[currentLocation], out var charMatch))
                    {
                        charMatch = 0;
                    }

                    if (computeMatches)
                    {
                        matchMask[currentLocation] = charMatch != 0 ? 1 : 0;
                    }

                    bitArr[j] = ((bitArr[j + 1] << 1) | 1) & charMatch;

                    if (i != 0)
                    {
                        bitArr[j] |= ((lastBitArr[j + 1] | lastBitArr[j]) << 1) | 1 | lastBitArr[j + 1];
                    }

                    if ((bitArr[j] & mask) != 0)
                    {
                        finalScore = BitapAlgorithm.ComputeScore(
                            pattern,
                            errors: i,
                            currentLocation: currentLocation,
                            expectedLocation: expectedLocation,
                            ignoreLocation: ignoreLocation
                        );

                        if (finalScore <= currentThreshold)
                        {
                            currentThreshold = finalScore;
                            bestLocation = currentLocation;
                            if (bestLocation <= expectedLocation)
                            {
                                break;
                            }

                            start = Math.Max(1, 2 * expectedLocation - bestLocation);
                        }
                    }
                }

                var score = BitapAlgorithm.ComputeScore(
                    pattern,
                    errors: i + 1,
                    currentLocation: expectedLocation,
                    expectedLocation: expectedLocation,
                    ignoreLocation: ignoreLocation
                );

                if (score > currentThreshold)
                {
                    break;
                }

                lastBitArr = bitArr;
            }

            var result = (true, Math.Max(0.001, finalScore), Array.Empty<Range>());
            if (computeMatches)
            {
                var indices = BitapAlgorithm.ConvertMaskToIndices(matchMask, minMatchCharLength);
                if (indices.Length == 0)
                    result.Item1 = false;
                else
                {
                    result.Item3 = indices;
                }
            }

            return result;
        }

        private static Dictionary<char, int> CreatePatternAlphabet(string pattern)
        {
            var @return = new Dictionary<char, int>();
            for (var i = 0; i < pattern.Length; i += 1)
            {
                @return[pattern[i]] = (@return.TryGetValue(pattern[i], out var mask) ? mask : 0) |
                                      (1 << (pattern.Length - i - 1));
            }

            return @return;
        }
    }
}