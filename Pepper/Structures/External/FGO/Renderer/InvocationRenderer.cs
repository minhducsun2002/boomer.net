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
    
    public class InvocationInformation
    {
        internal InvocationInformation() {}
        public MstFunc RawFunction;
        public string Effect = "";
        public Dictionary<string, string[]> Statistics = new();
        public string[] ExtraInformation = Array.Empty<string>();
        public ActSetInformation? ActSetInformation = null;
        public bool RequireOnField;
    }
    
    public partial class InvocationRenderer
    {
        private Dictionary<string, string[]> arguments;
        private bool single;
        private MstFunc function;
        private MasterDataMongoDBConnection MasterData;
        public TraitService Trait { get; set; }
        
        public InvocationRenderer(MstFunc function, Dictionary<string, string[]> arguments, MasterDataMongoDBConnection connection)
        {
            this.arguments = arguments;
            this.function = function;
            MasterData = connection;
            single = false;
        }
        
        

        public InvocationInformation Render()
        {
            if (!Enum.IsDefined(typeof(FunctionType), function.Type))
                return new InvocationInformation
                {
                    Effect = $"Function ID {function.ID}, type {function.Type}",
                    Statistics = arguments,
                    ExtraInformation = new[]
                    {
                        "This function type is not defined. Contact my owner for this."
                    }
                };

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
                Effect = $"**{typeName}** {(count == default ? "" : "of")} {string.Join(" / ", count ?? Array.Empty<string>())}".TrimEnd(),
                RawFunction = function,
                Statistics = statistics,
                ActSetInformation = actSetInformation,
                RequireOnField = onField
            };
            
            switch (type)
            {
                case FunctionType.AddState:
                case FunctionType.AddStateShort:
                {
                    // We are assuming AddState(Short) functions only refer to a single buff.
                    // I mean, there is only a single dataval tuple.
                    var buff = MasterData.ResolveBuff(function.Vals[0]);
                    var (effect, stats, extra) = SpecializedInvocationParser.AddState_Short(function, buff!, statistics, Trait);
                    output.Effect = effect;
                    output.Statistics = stats;
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
                    output.Effect = effect;
                    output.Statistics = stats;
                    output.ExtraInformation = new [] { extra };
                    break;
                }

                case FunctionType.GainNp:
                case FunctionType.LossNp:
                {
                    var values = statistics.Consume("Value").Select(value => $"**{int.Parse(value) / 100}**%").ToArray();
                    output.Effect = $"**{typeName}** by {string.Join(" / ", values)}";
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
                    output.Effect = $"**{typeName}** by {string.Join(" / ", values)}";
                    break;
                }

                case FunctionType.ShortenSkill:
                {
                    var values = statistics.Consume("Value").ToArray();
                    var turnText = values.Length > 1
                        ? "turn" + (values.Contains("1") ? "(s)" : "")
                        : "turn" + (values[0] == "1" ? "" : "s");
                    output.Effect = $"**{typeName}** by {string.Join(" / ", values.Select(value => $"**{value}**"))} {turnText}";
                    break;
                }

                case FunctionType.DamageNP:
                case FunctionType.DamageNPPierce:
                {
                    var values = statistics.Consume("Value").Select(value => $"**{int.Parse(value) / 10}**%").ToList();
                    output.Effect = $"**{typeName}** of {string.Join(" / ", values)}";
                    break;
                }
            }

            foreach (var key in new[] {"Rate"})
            {
                if (!statistics.ContainsKey(key)) continue;
                var chance = statistics.Consume(key).Distinct().Select(value => $"{float.Parse(value) / 10}%").ToArray();
                switch (chance.Length)
                {
                    case 1 when chance[0] != "100%":
                        output.Effect = $"{output.Effect} with **{chance[0]}** chance";
                        break;
                    case > 1:
                        output.Statistics["Chance to activate"] = chance;
                        break;
                }
                break;
            }

            return output;
        }
    }
}