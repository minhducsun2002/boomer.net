using System.Linq;
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

namespace Pepper.Commons.Osu.Test
{
    public class HitStatisticsSynthesizerTest
    {
        private const int HitObjectCount = 10000;
        private readonly HitStatisticsSynthesizer synthesizer;
        private readonly ITestOutputHelper output;
        private static readonly Ruleset[] Rulesets = { new OsuRuleset(), new TaikoRuleset(), new CatchRuleset(), new ManiaRuleset() };

        public HitStatisticsSynthesizerTest(ITestOutputHelper output)
        {
            synthesizer = new HitStatisticsSynthesizer(HitObjectCount);
            this.output = output;
        }

        [Theory]
        [InlineData(0.05)]
        [InlineData(0.15)]
        [InlineData(0.166666)]
        [InlineData(0.2)]
        [InlineData(0.333333)]
        [InlineData(0.4)]
        [InlineData(0.666666)]
        [InlineData(0.82)]
        [InlineData(0.9)]
        [InlineData((double) 300 / 305)]
        public void TestSynthesizedAccurateHits(double accuracy)
        {
            foreach (var ruleset in Rulesets)
            {
                var result = synthesizer.Synthesize(ruleset, accuracy);
                output.WriteLine(string.Join(", ", result.Select(kv => $"{kv.Key} : {kv.Value}")));

                // 0.1%, or 0.001 in difference
                Assert.InRange(result.Values.Sum(), (int) (HitObjectCount * 0.999), (int) (HitObjectCount * 1.001));
                Assert.All(result, kv => Assert.InRange(kv.Value, 0, HitObjectCount));

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
                Assert.Equal(scoreProcessor.Accuracy.Value, accuracy, 3);
            }
        }
    }
}