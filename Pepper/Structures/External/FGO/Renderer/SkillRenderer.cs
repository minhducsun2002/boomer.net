using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using MongoDB.Driver;
using Pepper.Services.FGO;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO.Renderer
{
    public class SkillRenderer : EntityRenderer<MstSkill>
    {
        private readonly MstSkill skillEntity;
        private readonly Lazy<MstSkillLv[]> lazyLevels;
        private readonly Lazy<MstFunc[]> lazyFunc;
        
        private MstSkillLv[] Levels => lazyLevels.Value;
        private MstFunc[] Functions => lazyFunc.Value;
        private int SkillId => skillEntity.ID;

        public SkillRenderer(MstSkill skill, MasterDataMongoDBConnection connection) : base(skill, connection)
        {
            skillEntity = skill;

            lazyLevels = new Lazy<MstSkillLv[]>(
                () => Connection.MstSkillLv
                    .FindSync(Builders<MstSkillLv>.Filter.Eq("skillId", SkillId))
                    .ToList()
                    .OrderBy(level => level.Level)
                    .ToArray()
            );
            
            lazyFunc = new Lazy<MstFunc[]>(
                () => Connection.MstFunc
                    .FindSync(Builders<MstFunc>.Filter.Or(
                        Levels[0].FuncToSvals.Keys.Select(
                            functionId => Builders<MstFunc>.Filter.Eq("id", functionId)
                        )
                    ))
                    .ToList().ToArray()
            );
        }

        public EmbedBuilder Prepare(TraitService trait)
        {
            var multipleActSet = false;
            var effects = Functions
                .ToDictionary(
                    function => function,
                    function =>
                    {
                        var statistics = Levels
                            .Select(level => level.FuncToSvals[function.ID])
                            .Select(raw => DataVal.Parse(raw, function.Type))
                            .SelectMany(dict => dict)
                            .ToLookup(pair => pair.Key, pair => pair.Value)
                            .ToDictionary(group => group.Key, group => group.ToArray());
                        var invocationInformation = new InvocationRenderer(function, statistics, Connection) { Trait = trait }
                            .Render();
                        if (invocationInformation.ActSetInformation != null) multipleActSet = true;
                        return invocationInformation;
                    }
                );
            return new EmbedBuilder
            {
                Title = skillEntity.Name,
                Description = multipleActSet ? "This skill contains multiple act set - only one will be executed." : "",
                Fields = effects.Select(kv =>
                {
                    var (func, invocationInformation) = kv;
                    var actSetInformation = invocationInformation.ActSetInformation;
                    var stats = invocationInformation.Statistics;
                    var limits = new List<string>();
                    foreach (var (limitKey, outputName) in new[] {("Turn", "turn"), ("Count", "time")})
                    {
                        stats.TryGetValue(limitKey, out var values);
                        values = values?.Distinct().ToArray();
                        if (values?.Length != 1) continue;
                        stats.Remove(limitKey);
                        var value = long.Parse(values[0]);
                        if (value > 0) limits.Add($"{value} {outputName}" + (value > 1 ? "s" : ""));
                    }

                    return new EmbedFieldBuilder
                    {
                        Name = (actSetInformation != null ? $"[Set {actSetInformation.Value.ActSetID} - Weight {actSetInformation.Value.ActSetWeight}] " : "") 
                               + $"[{func.ID}]"
                               + (limits.Count != 0 ? $" ({string.Join(", ", limits)})" : ""),
                        Value = $"{invocationInformation.Effect}"
                                + "\n"
                                + string.Join(
                                    '\n',
                                    stats.Select(entry => $"[**{entry.Key}**] : {string.Join(" / ", entry.Value.Distinct())}")
                                )
                    };
                }).ToList()
            };
        }
    }
}