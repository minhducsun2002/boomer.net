using System;
using System.Collections.Generic;
using System.Linq;
using Disqord;
using MongoDB.Driver;
using Pepper.Services.FGO;
using Pepper.Structures.External.FGO.Entities;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO.Renderer
{
    public class SkillRenderer : EntityRenderer<MstSkill>
    {
        private readonly Skill skill;

        public SkillRenderer(MstSkill skill, MasterDataMongoDBConnection connection,  Skill? skillHint = null) : base(skill, connection)
        {
            this.skill = skillHint ?? connection.GetSkillById(skill.ID, skill);
        }

        public LocalEmbed Prepare(TraitService trait)
        {
            var multipleActSet = false;
            var effects = skill.Invocations
                .ToDictionary(
                    function => function.Key,
                    function =>
                    {
                        var (mstFunc, dataVals) = function;
                        var statistics = dataVals
                            .SelectMany(dict => dict)
                            .ToLookup(pair => pair.Key, pair => pair.Value)
                            .ToDictionary(group => group.Key, group => group.ToArray());
                        var invocationInformation = new InvocationRenderer(mstFunc, statistics, Connection) { Trait = trait }
                            .Render();
                        if (invocationInformation.ActSetInformation != null) multipleActSet = true;
                        return invocationInformation;
                    }
                );
            return new LocalEmbed
            {
                Title = skill.MstSkill.Name,
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

                    return new LocalEmbedField
                    {
                        Name = (actSetInformation != null ? $"[Set {actSetInformation.Value.ActSetID} - Weight {actSetInformation.Value.ActSetWeight}] " : "") 
                               + $"[{func.ID}]"
                               + (limits.Count != 0 ? $" ({string.Join(", ", limits)})" : ""),
                        Value = $"{invocationInformation.Effect} to {TargetTypeText.ResolveText(invocationInformation.RawFunction.TargetType)}"
                                + (invocationInformation.RequireOnField ? "\nWearer must be on field to take effect." : "")
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