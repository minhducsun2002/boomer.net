using System.Collections.Generic;
using Pepper.Structures.External.FGO.Renderer;
using Xunit;

namespace Pepper.Test.FGO
{
    public class DataValParserTest
    {
        [Theory]
        [SkillLv]
        public void ParseDataValFromMstSkillLv(int funcType, int funcId, int skillId, int level, string raw, Dictionary<string, string> baseOutput)
        {
            var parsed = DataValParser.Parse(raw, funcType);
            Assert.All(
                baseOutput,
                kv => Assert.Equal(Assert.Contains(kv.Key, parsed as IReadOnlyDictionary<string, string>), kv.Value)
            );
        }
    }
}
