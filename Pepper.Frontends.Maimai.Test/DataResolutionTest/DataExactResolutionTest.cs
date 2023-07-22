using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using dotenv.net;
using Pepper.Commons.Maimai.Structures.Data.Enums;
using Pepper.Frontends.Maimai.Services;
using Pepper.Frontends.Maimai.Structures;
using Xunit;
using Xunit.Abstractions;

namespace Pepper.Frontends.Maimai.Test.DataResolutionTest
{
    public class DataExactResolutionTest : IClassFixture<DataFixture>
    {
        private readonly DataFixture dataFixture;
        private MaimaiDataService DataService => dataFixture.MaimaiDataService;
        private readonly ITestOutputHelper testOutputHelper;
        public DataExactResolutionTest(DataFixture fixture, ITestOutputHelper helper)
        {
            dataFixture = fixture;
            testOutputHelper = helper;
        }

        [Fact]
        public void ResolveExactWithNameAndDifficultyAndLevelAndVersionWorks()
        {
            var data = DataService.SongCache.SelectMany(
                kv => kv.Value.Difficulties
                    .Select(diff => (name: kv.Value.Name, id: kv.Key, diff: (Difficulty) diff.Order, level: diff.Level, level_decimal: diff.LevelDecimal))
            ).ToArray();
            Assert.NotEmpty(data);
            foreach (var (name, id, difficulty, level, levelDecimal) in data)
            {
                var version = id > 10000 ? ChartVersion.Deluxe : ChartVersion.Standard;
                var res = DataService.ResolveSongExact(name, difficulty, (level, levelDecimal >= 7), version);
                try
                {
                    Assert.NotNull(res);
                }
                catch
                {
                    testOutputHelper.WriteLine("Not found : {5}. {0} [{4}] [{1}] {2}.{3}", name, difficulty, level, levelDecimal, version, id);
                    throw;
                }
                var (diff, song) = res!.Value;

                (string, ChartLevel) expected = (name, new ChartLevel { Whole = level, Decimal = levelDecimal }), p1 = (song.Name, diff);
                Assert.Equal(expected, p1);
            }
        }
    }
}