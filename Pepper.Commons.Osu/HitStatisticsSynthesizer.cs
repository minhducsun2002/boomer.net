using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;

namespace Pepper.Commons.Osu
{
    /// <summary>
    /// A hit statistics synthesizer.
    /// <br />
    /// Given an accuracy, tries to synthesize hit counts for each type of hit result (300s, 200s, 100s, 50s, etc.)
    /// <br />
    /// It stays on the optimistic side - if there are multiple possibilities, the one with the higher number of better hits will almost always be returned.
    /// </summary>
    public class HitStatisticsSynthesizer
    {
        private readonly Beatmap<HitObject> beatmap;
        private const int MitigationMultiplier = 1000;

        /// <param name="hitobjectCount">The number of hit objects in the beatmap.</param>
        public HitStatisticsSynthesizer(int hitobjectCount)
        {
            beatmap = new Beatmap<HitObject>
            {
                HitObjects = Enumerable.Range(0, hitobjectCount).Select(_ => new HitCircle() as HitObject).ToList()
            };
        }

        private class NextScoreProcessor : ScoreProcessor
        {
            public NextScoreProcessor(Ruleset ruleset) : base(ruleset) { }
            public void Reset() => base.Reset(true);
        }

        /// <summary>
        /// Synthesize hit counts using valid hit results provided by a ruleset.
        /// </summary>
        /// <param name="ruleset">Ruleset used.</param>
        /// <param name="targetAccuracy">Desired accuracy. Must be between 0.0 and 1.0 inclusively.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="targetAccuracy"/> is outside the expected range.</exception>
        public Dictionary<HitResult, int> Synthesize(Ruleset ruleset, double targetAccuracy = 1.0)
        {
            if (targetAccuracy is > 1.0 or < 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(targetAccuracy),
                    targetAccuracy,
                    "accuracy must be in the 0.0-1.0 range"
                );
            }

            var resultContributions = GetContributions(ruleset);
            var internalTargetAccuracy = targetAccuracy * MitigationMultiplier;

            var output = new Dictionary<HitResult, int>();

            var baseHitCount = internalTargetAccuracy;

            for (var i = 0; i < resultContributions.Length; i++)
            {
                if (resultContributions[i].value == 0.0)
                {
                    output[resultContributions[i].result] = 0;
                    continue;
                }
                var countForThis = (int) Math.Floor(baseHitCount / resultContributions[i].value);
                output[resultContributions[i].result] = countForThis;
                baseHitCount -= countForThis * resultContributions[i].value;
            }

            var remaining = beatmap.HitObjects.Count - output.Values.Sum();

            for (var i = 1; i < resultContributions.Length; i++)
            {
                if (remaining <= 0)
                {
                    break;
                }

                var previousHit = resultContributions[i - 1];
                var currentHit = resultContributions[i];
                var convertRate = previousHit.value / currentHit.value;

                if (double.IsInfinity(convertRate))
                {
                    // contribution is 0 (misses?)
                    // just add them in i guess?
                    output[currentHit.result] = remaining;
                    break;
                }


                var pluses = convertRate - 1;
                var expectedConverts = (int) Math.Floor(remaining / pluses);

                // you can't convert more than what's available.
                var actualConverts = Math.Min(output[previousHit.result], expectedConverts);
                output[previousHit.result] -= actualConverts;
                output[currentHit.result] += (int) Math.Floor(actualConverts * convertRate);
                remaining += actualConverts;
                remaining -= output[currentHit.result];
            }

            // due to inaccuracies there might be missing hits. at this point let's just assume they were best hits possible.
            var missingHits = beatmap.HitObjects.Count - output.Values.Sum();
            if (missingHits > 0)
            {
                output[resultContributions[0].result] += missingHits;
            }

            return output;
        }

        /// <summary>
        /// Get "contributions" towards accuracy.
        /// <br/>
        /// Contribution of a hit is defined as the amount of "accuracy" it gives. Sum of all hits in the map is the final accuracy.
        /// <br />
        /// For example, in osu!, SS a map containing 2 hitcircles requires 2 300-hits - contribution of such a hit is 0.5.
        /// <br />
        /// A 100-hit would then be worth 1/3 as much, or ~0.1667.
        /// </summary>
        /// <returns></returns>
        private (HitResult result, double value)[] GetContributions(Ruleset ruleset)
        {
            var missJudgement = new JudgementResult(beatmap.HitObjects[0], beatmap.HitObjects[0].CreateJudgement())
            {
                Type = HitResult.Miss
            };

            var scoreProcessor = new NextScoreProcessor(ruleset);
            scoreProcessor.ApplyBeatmap(beatmap);
            var validHitResults = ruleset
                .GetHitResults()
                .Select(r => r.result)
                .Where(r => !r.IsBonus() && r.IsBasic());

            var resultContribution = validHitResults
                .Select(result =>
                {
                    scoreProcessor.Reset();
                    scoreProcessor.ApplyResult(
                        new JudgementResult(beatmap.HitObjects[0], beatmap.HitObjects[0].CreateJudgement())
                        {
                            Type = result
                        }
                    );

                    for (var i = 1; i <= beatmap.HitObjects.Count - 1; i++)
                    {
                        scoreProcessor.ApplyResult(missJudgement);
                    }
                    return (result, value: scoreProcessor.Accuracy.Value * MitigationMultiplier);
                })
                .OrderByDescending(s => s.value)
                .ToArray();
            return resultContribution;
        }
    }
}