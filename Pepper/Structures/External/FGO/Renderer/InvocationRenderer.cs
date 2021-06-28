using System;
using System.Collections.Generic;
using System.Linq;
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
        public ActSetInformation? ActSetInformation;
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
            ActSetInformation? actSetInformation = null;
            if (statistics.ContainsKey("ActSet"))
            {
                actSetInformation = new ActSetInformation
                {
                    ActSetID = int.Parse(statistics["ActSet"].Distinct().First()),
                    ActSetWeight = int.Parse(statistics["ActSetWeight"].Distinct().First())
                };
                statistics.Remove("ActSet");
                statistics.Remove("ActSetWeight");
            }


            switch (type)
            {
                case FunctionType.AddState:
                case FunctionType.AddStateShort:
                    // We are assuming AddState(Short) functions only refer to a single buff.
                    // I mean, there is only a single dataval tuple.
                    var buff = MasterData.ResolveBuff(function.Vals[0]);
                    var (effect, stats, extra) = SpecializedInvocationParser.AddState_Short(function, buff!, statistics, Trait);
                    return new InvocationInformation
                    {
                        Effect = effect,
                        RawFunction = function,
                        Statistics = stats,
                        ExtraInformation = extra,
                        ActSetInformation = actSetInformation
                    };
                default:
                {
                    return new InvocationInformation
                    {
                        Effect = $"**{typeName}**".TrimEnd(),
                        RawFunction = function,
                        Statistics = statistics,
                        ActSetInformation = actSetInformation
                    };
                }
            }
        }
    }
}