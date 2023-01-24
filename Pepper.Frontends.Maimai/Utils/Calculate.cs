using System.Runtime.CompilerServices;

namespace Pepper.Frontends.Maimai.Utils
{
    public static class Calculate
    {
        /// <param name="accuracy">Accuracy, in range [0, 1005000]</param>
        /// <param name="chartConstant">Chart constant, in range [10, 150]</param>
        /// <returns></returns>
        public static long GetFinalScore(long accuracy, long chartConstant)
        {
            if (accuracy > 1005000)
            {
                accuracy = 1005000;
            }
            return accuracy * chartConstant * GetRankCoeff(accuracy);
        }

        public static readonly (int, int)[] Coeff = {
            (1005000, 224),
            (1004999, 222),
            (1000000, 216),
            (0999999, 214),
            (0995000, 211),
            (0990000, 208),
            (0989999, 206),
            (0980000, 203),
            (0970000, 200),
            (0969999, 176),
            (0940000, 168),
            (0900000, 152),
            (0800000, 136),
            (0799999, 128),
            (0750000, 120),
            (0700000, 112),
            (0600000, 096),
            (0500000, 080),
            (0400000, 064),
            (0300000, 048),
            (0200000, 032),
            (0100000, 016)
        };

        private static int GetRankCoeff(long accuracy)
        {
            for (var i = 0; i < Coeff.Length; i++)
            {
                if (accuracy >= Coeff[i].Item1)
                {
                    return Coeff[i].Item2;
                }
            }

            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int NormalizedRating(long total)
        {
            return (int) (total / 1000000 / 10 / 10);
        }
    }
}