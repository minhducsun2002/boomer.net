using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using dotenv.net;
using Pepper.Commons.Maimai.Structures.Data.Enums;
using Pepper.Frontends.Maimai.Services;
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
        public void ResolveExactWithIdAndDifficultyWorks()
        {
            var data = DataService.SongCache.SelectMany(
                kv => kv.Value.Difficulties
                    .Select(diff => (id: kv.Key, diff: (Difficulty) diff.Order))
            ).ToArray();
            Assert.NotEmpty(data);
            foreach (var (id, difficulty) in data)
            {
                var res = DataService.ResolveSongExact(id, difficulty);
                Assert.NotNull(res);
                var (diff, song) = res!.Value;

                (int, Difficulty) expected = (id, difficulty), p1 = (song.Id, (Difficulty) diff.Order);
                Assert.Equal(expected, p1);
            }
        }

        [Fact]
        public void ResolveExactWithNameAndDifficultyAndLevelWorks()
        {
            var data = DataService.SongCache.SelectMany(
                kv => kv.Value.Difficulties
                    .Select(diff => (name: kv.Value.Name, id: kv.Key, diff: (Difficulty) diff.Order, level: diff.Level, level_decimal: diff.LevelDecimal))
            ).ToArray();
            Assert.NotEmpty(data);
            foreach (var (name, id, difficulty, level, levelDecimal) in data)
            {
                var res = DataService.ResolveSongExact(name, difficulty, (level, levelDecimal >= 7));
                try
                {
                    Assert.NotNull(res);
                }
                catch
                {
                    testOutputHelper.WriteLine("Not found : {4}. {0} [{1}] {2}.{3}", name, difficulty, level, levelDecimal, id);
                    throw;
                }
                var (diff, song) = res!.Value;

                (string, Difficulty, int, int) expected = (name, difficulty, diff.Level, diff.LevelDecimal),
                    p1 = (song.Name, (Difficulty) diff.Order, diff.Level, diff.LevelDecimal);
                Assert.Equal(expected, p1);
            }
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

                (string, Difficulty, int, int) expected = (name, difficulty, diff.Level, diff.LevelDecimal),
                    p1 = (song.Name, (Difficulty) diff.Order, diff.Level, diff.LevelDecimal);
                Assert.Equal(expected, p1);
            }
        }
    }
}