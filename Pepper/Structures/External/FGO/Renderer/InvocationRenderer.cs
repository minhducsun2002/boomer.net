using System;
using System.Collections.Generic;
using System.Linq;
using FFmpeg.AutoGen;
using MongoDB.Driver;
using Pepper.Services.FGO;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO.Renderer
{
    public struct ActSetInformation
    {
        public int ActSetID;
        public int ActSetWeight;
    }

    public enum TreasureDeviceMutationType
    {
        Level,
        Overcharge
    }
    
    public class InvocationInformation
    {
        internal InvocationInformation() {}

        public string Effect { get; private set; } = "";
        public TreasureDeviceMutationType? EffectMutationType { get; private set;  } = null;
        public InvocationInformation WithEffect(string effect, TreasureDeviceMutationType? effectMutationType = null)
        {
            Effect = effect;
            EffectMutationType = effectMutationType;
            return this;
        }
        
        public MstFunc RawFunction;
        public Dictionary<string, string[]> Statistics = new();
        public Dictionary<string, TreasureDeviceMutationType> TreasureDeviceMutationTypeHint = new();
        public string[] ExtraInformation = Array.Empty<string>();
        public ActSetInformation? ActSetInformation = null;
        public bool RequireOnField;
    }

    internal static class MutationTypeHelperExtensions
    {
        internal static TreasureDeviceMutationType? ResolveMutationType(
            this IReadOnlyDictionary<string, TreasureDeviceMutationType> mutationTypeHint,
            string key
        )
            => mutationTypeHint.TryGetValue(key, out var mutationType) ? mutationType : null;
    }
    
    public partial class InvocationRenderer
    {
        private Dictionary<string, string[]> arguments;
        private bool single;
        private MstFunc function;
        private MasterDataMongoDBConnection MasterData;
        private readonly TraitService traitService;
        
        public InvocationRenderer(MstFunc function, Dictionary<string, string[]> arguments, MasterDataMongoDBConnection connection, TraitService traitService)
        {
            this.arguments = arguments;
            this.function = function;
            this.traitService = traitService;
            MasterData = connection;
            single = false;
        }
        

        public InvocationInformation Render(Dictionary<string, TreasureDeviceMutationType>? mutationTypeHint = null)
        {
            mutationTypeHint ??= new(); 
            if (!Enum.IsDefined(typeof(FunctionType), function.Type))
                return new InvocationInformation
                {
                    Statistics = arguments,
                    ExtraInformation = new[]
                    {
                        "This function type is not defined. Contact my owner for this."
                    }
                }.WithEffect($"Function ID {function.ID}, type {function.Type}");

            var type = (FunctionType) function.Type;
            if (!FunctionNames.TryGetValue(type, out var typeName)) typeName = "";
            if (string.IsNullOrWhiteSpace(typeName)) typeName = $"[functionType {type}]";
            var statistics = arguments;
            var onField = false;
            
            ActSetInformation? actSetInformation = null;
            if (statistics.ContainsKey("ActSet"))
                actSetInformation = new ActSetInformation
                {
                    ActSetID = int.Parse(statistics.Consume("ActSet").Distinct().First()),
                    ActSetWeight = int.Parse(statistics.Consume("ActSetWeight").Distinct().First())
                };
            
            if (statistics.Remove("OnField")) onField = true;
            statistics.Remove("HideMiss"); // 960407

            statistics.TryGetValue("Count", out var count);
            var output = new InvocationInformation
            {
                RawFunction = function,
                Statistics = statistics,
                ActSetInformation = actSetInformation,
                RequireOnField = onField
            }.WithEffect(
                $"**{typeName}** {(count == default ? "" : "of")} {string.Join(" / ", count ?? Array.Empty<string>())}".TrimEnd(),
                mutationTypeHint.ResolveMutationType("Count")
            );
            
            switch (type)
            {
                case FunctionType.AddState:
                case FunctionType.AddStateShort:
                {
                    // We are assuming AddState(Short) functions only refer to a single buff.
                    // I mean, there is only a single dataval tuple.
                    var buff = MasterData.ResolveBuff(function.Vals[0]);
                    var (effect, stats, extra, mutationTypes) = 
                        SpecializedInvocationParser.AddState_Short(function, buff!, statistics, traitService, mutationTypeHint);
                    output = output.WithEffect(effect, null);
                    output.Statistics = stats;
                    output.TreasureDeviceMutationTypeHint = mutationTypes;
                    output.ExtraInformation = extra;
                    break;
                }

                case FunctionType.EventDropUp:
                {
                    // We are assuming the individuality refers to only one item
                    // We are also assuming that individuality does not change.
                    int individualty = int.Parse(statistics["Individuality"][0]),
                        eventId = int.Parse(statistics["EventId"][0]);
                    var @event = MasterData.MstEvent.FindSync(Builders<MstEvent>.Filter.Eq("id", eventId)).First();
                    var item = MasterData.MstItem.FindSync(Builders<MstItem>.Filter.Eq("individuality", individualty)).First();
                    var (effect, stats, extra) = SpecializedInvocationParser.EventDropUp(function, @event, item, statistics);
                    // We are assuming a NP will not carry anything event-related,
                    output = output.WithEffect(effect);
                    output.Statistics = stats;
                    output.ExtraInformation = new [] { extra };
                    break;
                }

                case FunctionType.GainNp:
                case FunctionType.LossNp:
                {
                    var values = statistics.Consume("Value").Select(value => $"**{int.Parse(value) / 100}**%").ToArray();
                    output = output.WithEffect($"**{typeName}** by {string.Join(" / ", values)}",
                        mutationTypeHint.ResolveMutationType("Value"));
                    output.Statistics = statistics;
                    break;
                }

                case FunctionType.GainStar:
                case FunctionType.GainHp:
                case FunctionType.LossHp:
                case FunctionType.LossHpSafe:
                case FunctionType.HastenNpturn:
                {
                    var values = statistics.Consume("Value").Distinct().Select(value => $"**{value}**");
                    output.WithEffect($"**{typeName}** by {string.Join(" / ", values)}",
                        mutationTypeHint.ResolveMutationType("Value"));
                    break;
                }

                case FunctionType.ShortenSkill:
                {
                    var values = statistics.Consume("Value").ToArray();
                    var turnText = values.Length > 1
                        ? "turn" + (values.Contains("1") ? "(s)" : "")
                        : "turn" + (values[0] == "1" ? "" : "s");
                    output = output.WithEffect(
                        $"**{typeName}** by {string.Join(" / ", values.Select(value => $"**{value}**"))} {turnText}",
                        mutationTypeHint.ResolveMutationType("Value")
                    );
                    break;
                }

                case FunctionType.DamageNP:
                case FunctionType.DamageNPPierce:
                {
                    var values = statistics.Consume("Value").Select(value => $"**{int.Parse(value) / 10}**%").ToList();
                    output = output.WithEffect(
                        $"**{typeName}** of {string.Join(" / ", values)}",
                        mutationTypeHint.ResolveMutationType("Value")
                    );
                    break;
                }

                case FunctionType.DamageNPIndividual:
                case FunctionType.DamageNPStateIndividualFix:
                {
                    var values = statistics.Consume("Value").Select(value => $"**{int.Parse(value) / 10}**%").ToList();
                    output = output.WithEffect(
                        $"**{typeName}** of {string.Join(" / ", values)}",
                        mutationTypeHint.ResolveMutationType("Value")
                    );

                    var specialDamageValue = statistics.Consume("Correction")
                        .Select(value => $"**{int.Parse(value) / 10}**%").ToArray();
                    var trait = traitService.GetTrait(int.Parse(statistics.Consume("Target").First()));
                    
                    var mutationType = mutationTypeHint.ResolveMutationType("Correction");
                    if (mutationType.HasValue) output.TreasureDeviceMutationTypeHint[$"Special damage for {trait}"] = mutationType.Value;
                    
                    output.Statistics[$"Special damage for {trait}"] = specialDamageValue;
                    break;
                }
            }

            if (statistics.ContainsKey("Rate"))
            {
                var chance = statistics.Consume("Rate").Distinct().Select(value => $"{float.Parse(value) / 10}%").ToArray();
                switch (chance.Length)
                {
                    case 1 when chance[0] != "100%":
                        output.WithEffect($"{output.Effect} with **{chance[0]}** chance", null);
                        break;
                    case > 1:
                        output.Statistics["Chance to activate"] = chance;
                        var mutationType = mutationTypeHint.ResolveMutationType("Rate");
                        if (mutationType != null)
                            output.TreasureDeviceMutationTypeHint["Chance to activate"] = mutationType.Value;
                        break;
                }
            }

            return output;
        }
    }
}