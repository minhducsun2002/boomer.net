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
    public static class SkillEffectExtensions
    {
        public static string Serialize(this KeyValuePair<MstFunc, InvocationInformation> kv)
        {
            var (_, invocationInformation) = kv;
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

            return
                (actSetInformation != null ? $"[Act set {actSetInformation.Value.ActSetID} - Weight {actSetInformation.Value.ActSetWeight}]\n" : "")
                + $"{invocationInformation.Effect} to **{TargetTypeText.ResolveText(invocationInformation.RawFunction.TargetType)}**"
                + (limits.Count != 0 ? $" ({string.Join(", ", limits)})" : "")
                + (invocationInformation.RequireOnField ? "\nWearer must be on field to take effect." : "")
                + (invocationInformation.ExtraInformation.Length != 0
                    ? "\n" + string.Join('\n', invocationInformation.ExtraInformation)
                    : "")
                + (stats.Count != 0
                    ? "\n" + string.Join(
                        '\n',
                        stats.Select(entry => $"[**{entry.Key}**] : {string.Join(" / ", entry.Value.Distinct())}")
                    )
                    : "");
        }
    }
    
    public class SkillRenderer : EntityRenderer<MstSkill>
    {
        private readonly Skill skill;

        public SkillRenderer(MstSkill skill, IMasterDataProvider connection,  Skill? skillHint = null) : base(skill, connection)
        {
            this.skill = skillHint ?? connection.GetSkillById(skill.ID, skill)!;
        }

        public (List<string>, List<(Skill, List<string>)>) Prepare(TraitService trait, bool hideEnemy = false)
        {
            var (effects, multipleActSet) = ResolveEffects(trait);

            var _ = new HashSet<int>();
            var referencedSkillIds = new SkillReferenceEnumerator(skill, Connection, ref _).Enumerate();
            referencedSkillIds.Remove(skill.MstSkill.ID);
            var skills = referencedSkillIds.Select(referencedSkillId =>
            {
                var referencedSkill = Connection.GetSkillById(referencedSkillId);
                var serializedReferencedEffects = new SkillRenderer(referencedSkill!.MstSkill, Connection, referencedSkill)
                    .ResolveEffects(trait).Item1
                    .Where(kv => !hideEnemy || EnemyActionFilter.IsPlayerAction(kv.Key))
                    .Select(kv => kv.Serialize());
                return (referencedSkill, serializedReferencedEffects.ToList());
            }).ToList();

            var serializedEffects = effects
                .Where(kv => !hideEnemy || EnemyActionFilter.IsPlayerAction(kv.Key))
                .Select(kv => kv.Serialize()).ToList();
            return (serializedEffects, skills);
        }
        
        public (Dictionary<MstFunc, InvocationInformation>, bool) ResolveEffects(TraitService trait)
        {
            var multipleActSet = false;
            var @out =  skill.Invocations
                .ToDictionary(
                    function => function.Key,
                    function =>
                    {
                        var (mstFunc, dataVals) = function;
                        var statistics = dataVals
                            .SelectMany(dict => dict)
                            .ToLookup(pair => pair.Key, pair => pair.Value)
                            .ToDictionary(group => group.Key, group => group.ToArray());
                        var invocationInformation = new InvocationRenderer(mstFunc, statistics, Connection, trait)
                            .Render();
                        if (invocationInformation.ActSetInformation != null) multipleActSet = true;
                        return invocationInformation;
                    }
                );
            return (@out, multipleActSet);
        }
    }
}