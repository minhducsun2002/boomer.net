using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Taiko;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Pepper.Commons.Osu.Test
{
    public class HitStatisticsSynthesizerTest
    {
        private const int HitObjectCount = 10000;
        private readonly HitStatisticsSynthesizer synthesizer;
        private readonly ITestOutputHelper output;

        public HitStatisticsSynthesizerTest(ITestOutputHelper output)
        {
            synthesizer = new HitStatisticsSynthesizer(HitObjectCount);
            this.output = output;
        }

        [Theory]
        [AccuracyData(0.10)]
        public void TestSynthesizedEnoughHits(double accuracy, Ruleset ruleset)
        {
            var result = synthesizer.Synthesize(ruleset, accuracy);
            output.WriteLine(string.Join(", ", result.Select(kv => $"{kv.Key} : {kv.Value}")));
            // 0.1%, or 0.001 in difference
            Assert.InRange(result.Values.Sum(), (int) (HitObjectCount * 0.999), (int) (HitObjectCount * 1.001));
        }

        [Theory]
        [AccuracyData(0.10)]
        public void TestSynthesizedPossibleHits(double accuracy, Ruleset ruleset)
        {
            var result = synthesizer.Synthesize(ruleset, accuracy);
            output.WriteLine(string.Join(", ", result.Select(kv => $"{kv.Key} : {kv.Value}")));
            Assert.All(result, kv => Assert.InRange(kv.Value, 0, HitObjectCount));
        }

        [Theory]
        [AccuracyData(0.10)]
        public void TestSynthesizedAccurateResult(double accuracy, Ruleset ruleset)
        {
            var result = synthesizer.Synthesize(ruleset, accuracy);
            var beatmap = new Beatmap<HitObject>
            {
                HitObjects = Enumerable.Range(0, HitObjectCount).Select(_ => new HitCircle() as HitObject).ToList()
            };
            var scoreProcessor = ruleset.CreateScoreProcessor();
            scoreProcessor.ApplyBeatmap(beatmap);
            foreach (var (hitResult, times) in result)
            {
                for (var i = 0; i < times; i++)
                {
                    var judgement = new JudgementResult(beatmap.HitObjects[0], beatmap.HitObjects[0].CreateJudgement())
                    {
                        Type = hitResult
                    };
                    scoreProcessor.ApplyResult(judgement);
                }
            }
            Assert.Equal(scoreProcessor.Accuracy.Value, accuracy, 4);
        }

        private class AccuracyDataAttribute : DataAttribute
        {
            private readonly double start, end, step;
            private static readonly Ruleset[] Rulesets =
            {
                new OsuRuleset(),
                new TaikoRuleset(),
                new CatchRuleset(),
                new ManiaRuleset()
            };

            public AccuracyDataAttribute(double start = 0.9, double end = 1.0, double step = 0.01)
            {
                this.start = start; this.end = end; this.step = step;
            }

            public override IEnumerable<object[]> GetData(MethodInfo _)
            {
                foreach (var ruleset in Rulesets)
                {
                    for (var i = start; i <= end; i += step)
                    {
                        yield return new object[] { i, ruleset };
                    }
                }
            }
        }
    }
}