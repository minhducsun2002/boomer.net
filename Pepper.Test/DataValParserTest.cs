using System.Collections.Generic;
using System.Linq;
using Pepper.Structures.External.FGO.MasterData;
using Pepper.Structures.External.FGO.Renderer;
using Xunit;

namespace Pepper.Test
{
    public class DataValParserTest
    {
        [Theory]
        [SkillLv]
        public void ParseDataValFromMstSkillLv(MstFunc func, string raw, Dictionary<string, string> baseOutput, MstSkillLv skillLv)
        {
            var parsed = DataValParser.Parse(raw, func.Type);
            var compare = parsed.OrderBy(kv => kv.Key)
                .SequenceEqual(baseOutput.OrderBy(kv => kv.Key));
            Assert.True(
                compare,
                $"Parsing failed : mismatched output - skill {skillLv.SkillId}, level {skillLv.Level}, function {func.ID}"
                );
        }
    }
}
