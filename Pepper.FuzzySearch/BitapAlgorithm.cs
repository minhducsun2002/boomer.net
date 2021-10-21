using System;
using System.Collections.Generic;
using System.Linq;

namespace Pepper.FuzzySearch
{
    internal static class BitapAlgorithm
    {
        public static double ComputeScore(
            string pattern,
            int errors = 0,
            bool ignoreLocation = false,
            int currentLocation = 0, int expectedLocation = 0,
            int distance = 100)
        {
            var accuracy = (double) errors / pattern.Length;
            if (ignoreLocation)
            {
                return accuracy;
            }

            var proximity = Math.Abs(expectedLocation - currentLocation);

            if (distance == 0)
            {
                return proximity != 0 ? 1.0D : accuracy;
            }

            return accuracy + (double) proximity / distance;
        }

        public static Range[] ConvertMaskToIndices(int[] matchMask, int minMatchCharLength = 1)
        {
            int start = -1, end = -1, i = 0;
            List<Range> indices = new();
            for (var len = matchMask.Length; i < len; i += 1)
            {
                var match = matchMask[i];
                if (match != 0 && start == -1)
                {
                    start = i;
                }
                else if (match == 0 && start != -1)
                {
                    end = i - 1;
                    if (end - start + 1 >= minMatchCharLength)
                    {
                        indices.Add(new Range(start, end));
                    }

                    start = -1;
                }
            }

            if (matchMask.ElementAtOrDefault(i - 1) != 0 && i - start >= minMatchCharLength)
            {
                indices.Add(new Range(start, i - 1));
            }

            return indices.ToArray();
        }
    }
}