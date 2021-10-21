using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Conventions;
using Pepper.Structures.External.FGO.MasterData;
using Pepper.Test.FGO;
using Xunit.Sdk;

namespace Pepper.Test
{
    public class ParsedMstSkillLv : MstSkillLv
    {
        [BsonElement("svalsParsed")] public Dictionary<string, string>[] Parsed = Array.Empty<Dictionary<string, string>>();
    }

    public class SkillLvAttribute : DataAttribute
    {
        private static readonly string ParsedName = "parsed_mstSkillLv.json";
        private static readonly string FuncName = $"{Names.MstFunc}.json";

        static SkillLvAttribute()
        {
            ConventionRegistry.Register(
                "IgnoreExtraElements",
                new ConventionPack { new IgnoreExtraElementsConvention(true) },
                _ => true
            );
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            var @out = Names.Versions.SelectMany(version =>
                {
                    var path = Path.Combine(Names.BasePath, version);
                    var parsedJson = File.ReadAllText(Path.Combine(path, ParsedName));
                    var funcJson = File.ReadAllText(Path.Combine(path, FuncName));
                    var mstSkillLvParsed = BsonSerializer.Deserialize<ParsedMstSkillLv[]>(parsedJson);
                    var mstFunc = BsonSerializer.Deserialize<MstFunc[]>(funcJson)
                        .ToDictionary(func => func.ID, func => func);

                    var _ = mstSkillLvParsed.SelectMany(
                        skillLv =>
                            skillLv.FuncId.Zip(skillLv.Svals).Zip(skillLv.Parsed)
                                .Where(zip => mstFunc.ContainsKey(zip.First.First))     // filter for existent function IDs
                                                                                        // ((funcId, sval), parsed)
                                .Select(zip =>
                                    (mstFunc[zip.First.First].Type, zip.First.First, skillLv.SkillId, skillLv.Level, zip.First.Second, zip.Second))
                    // (funcType, sval, parsed)
                    );
                    return _;
                })
                .Select(testCase => new object[] { testCase.Item1, testCase.Item2, testCase.Item3, testCase.Item4, testCase.Item5, testCase.Item6 });
            return @out;
        }
    }
}