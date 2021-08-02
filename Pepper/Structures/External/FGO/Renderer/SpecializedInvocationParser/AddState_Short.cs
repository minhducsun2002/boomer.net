using System;
using System.Collections.Generic;
using System.Linq;
using FgoExportedConstants;
using Pepper.Services.FGO;
using Pepper.Structures.External.FGO.MasterData;
using Pepper.Utilities;

namespace Pepper.Structures.External.FGO.Renderer
{
    internal class HumanizedEntry
    {
        public string[] Chance = Array.Empty<string>();
        public int[] Turn = Array.Empty<int>();
        public int[] Count = Array.Empty<int>();
        public string[] Amount = Array.Empty<string>();
    }
    
    public static partial class SpecializedInvocationParser
    {
        public static (string, Dictionary<string, string[]>, string[]) AddState_Short(
            MstFunc function, MstBuff buff, Dictionary<string, string[]> values,
            TraitService traitService
        )
        {
            var funcType = function.Type;
            var output = new HumanizedEntry();
            var extra = new List<string>();
            var extraStats = new Dictionary<string, string[]>();
            var amountPreposition = "of";
            string? buffName = default;

            var baseAction = funcType switch
            {
                (int) FunctionType.AddState => InvocationRenderer.FunctionNames[FunctionType.AddState],
                (int) FunctionType.AddStateShort => InvocationRenderer.FunctionNames[FunctionType.AddStateShort],
                _ => throw new ArgumentException(
                    $"{nameof(funcType)} must be either AddState or AddStateShort. Received {funcType}")
            };
            
            if (Enum.IsDefined(typeof(BuffList.TYPE), buff.Type))
                if (!InvocationRenderer.BuffNames.TryGetValue((BuffList.TYPE) buff.Type, out buffName))
                    buffName = $"[buffType ${buff.Type}]";
            
            if (values.ContainsKey("Turn")) output.Turn = values["Turn"].Distinct().Select(int.Parse).ToArray();

            if (Enum.IsDefined(typeof(BuffList.TYPE), buff.Type))
                switch ((BuffList.TYPE) buff.Type)
                {
                    case BuffList.TYPE.UP_TOLERANCE:     case BuffList.TYPE.DOWN_TOLERANCE:
                    case BuffList.TYPE.UP_COMMANDALL:    case BuffList.TYPE.DOWN_COMMANDALL:
                    case BuffList.TYPE.UP_GRANTSTATE:    case BuffList.TYPE.DOWN_GRANTSTATE:
                    case BuffList.TYPE.UP_CRITICALPOINT: case BuffList.TYPE.DOWN_CRITICALPOINT:
                    case BuffList.TYPE.UP_CRITICALRATE:  case BuffList.TYPE.DOWN_CRITICALRATE:
                    case BuffList.TYPE.UP_CRITICALDAMAGE:case BuffList.TYPE.DOWN_CRITICALDAMAGE:
                    case BuffList.TYPE.UP_DAMAGE:        case BuffList.TYPE.DOWN_DAMAGE:
                    case BuffList.TYPE.UP_GAIN_HP:       case BuffList.TYPE.DOWN_GAIN_HP:
                    case BuffList.TYPE.UP_DEFENCE:       case BuffList.TYPE.DOWN_DEFENCE:
                    case BuffList.TYPE.UP_GIVEGAIN_HP:
                    case BuffList.TYPE.UP_DAMAGEDROPNP:  case BuffList.TYPE.DOWN_DAMAGEDROPNP:
                    case BuffList.TYPE.UP_DROPNP:        case BuffList.TYPE.DOWN_DROPNP:
                    case BuffList.TYPE.UP_RESIST_INSTANTDEATH:
                    case BuffList.TYPE.UP_NPDAMAGE:      case BuffList.TYPE.DOWN_NPDAMAGE:
                    case BuffList.TYPE.UP_COMMANDATK:    case BuffList.TYPE.DOWN_COMMANDATK:
                    case BuffList.TYPE.UP_STARWEIGHT:    case BuffList.TYPE.DOWN_STARWEIGHT:
                    case BuffList.TYPE.UP_GRANT_INSTANTDEATH:
                    case BuffList.TYPE.UP_FUNC_HP_REDUCE:
                    case BuffList.TYPE.UP_ATK:           case BuffList.TYPE.DOWN_ATK:
                    case BuffList.TYPE.UP_TOLERANCE_SUBSTATE:
                    case BuffList.TYPE.GUTS_RATIO:
                    case BuffList.TYPE.DOWN_DEFENCECOMMANDALL:
                        output.Count = values["Count"].Select(int.Parse).ToArray();
                        output.Amount = values["Value"].Select(value => $"{float.Parse(value) / 10}%").ToArray();
                        break;
                    case BuffList.TYPE.REDUCE_HP:        case BuffList.TYPE.REGAIN_HP:
                    case BuffList.TYPE.GUTS:
                    case BuffList.TYPE.UP_CHAGETD:
                        output.Count = values["Count"].Select(int.Parse).ToArray();
                        output.Amount = values["Value"].Select(value => $"{int.Parse(value)}").ToArray();
                        break;
                    case BuffList.TYPE.AVOID_INSTANTDEATH:
                        output.Count = values["Count"].Select(value => int.Parse(value) / 10).ToArray();
                        break;
                    case BuffList.TYPE.REGAIN_NP:
                        output.Amount = values["Value"].Select(value => $"{float.Parse(value) / 10}%").ToArray();
                        break;
                    case BuffList.TYPE.ADD_DAMAGE:
                    case BuffList.TYPE.REGAIN_STAR:
                    case BuffList.TYPE.UP_DAMAGE_INDIVIDUALITY_ACTIVEONLY:
                    case BuffList.TYPE.ADD_MAXHP:        case BuffList.TYPE.SUB_MAXHP:
                    case BuffList.TYPE.OVERWRITE_CLASSRELATIO_ATK:
                    case BuffList.TYPE.SUB_SELFDAMAGE:
                        output.Amount = values["Value"].Select(value => $"{int.Parse(value)}").ToArray();
                        break;
                    case BuffList.TYPE.MULTIATTACK:
                        amountPreposition = "by ";
                        output.Amount = values["Value"].Select(value => $"{int.Parse(value)}").ToArray();
                        break;
                    case BuffList.TYPE.COMMANDATTACK_FUNCTION:
                        // We are assuming these buffs only push a single skill
                        buffName = $"Trigger skill {values["Value"].First()} on attacks";
                        var triggerChances = values["UseRate"].Distinct().Select(chance => int.Parse(chance) / 10).ToList();
                        switch (triggerChances.Count)
                        {
                            case 1:
                                buffName = $"Trigger skill {values["Value"].First()} with {triggerChances[0]}% chance on attacks";
                                break;
                            case > 1:
                                extraStats["Skill trigger chance"] = triggerChances.Select(chance => $"{chance}%").ToArray();
                                break;
                        }
                        break;
                    case BuffList.TYPE.DEAD_FUNCTION:
                    case BuffList.TYPE.DELAY_FUNCTION:
                    case BuffList.TYPE.ENTRY_FUNCTION:
                    case BuffList.TYPE.GUTS_FUNCTION:
                    case BuffList.TYPE.NPATTACK_PREV_BUFF:
                    case BuffList.TYPE.SELFTURNEND_FUNCTION:
                        break;
                    case BuffList.TYPE.AVOIDANCE:
                    case BuffList.TYPE.INVINCIBLE:
                        output.Count = values["Count"].Select(int.Parse).ToArray();
                        break;
                    case BuffList.TYPE.CHANGE_COMMAND_CARD_TYPE:
                    case BuffList.TYPE.ADD_INDIVIDUALITY:
                        extra.Add(
                            "Change all Command Cards of the target to"
                            + values["Value"].Select(card => traitService.GetTrait(int.Parse(card) + 4000))
                        );
                        break;
                }

            output.Amount = output.Amount.Distinct().ToArray(); 
            output.Chance = output.Chance.Distinct().ToArray();
            output.Turn = output.Turn.Distinct().ToArray();
            output.Count = output.Count.Distinct().ToArray();
            
            string amount = "", limits = "";

            foreach (var (limitType, data) in new[] {("turn", output.Turn), ("time", output.Count)})
            {
                if (data.Length != 1) continue;
                if (data[0] < 1) continue;
                limits += (string.IsNullOrEmpty(limits) ? "" : ", ") + $"**{data[0]}** {limitType}{(data[0] > 1 ? "s" : "")}";
            }
            
            switch (output.Amount.Length)
            {
                case 1:
                    amount = output.Amount[0];
                    output.Amount = Array.Empty<string>();
                    break;
                case > 1:
                    extraStats[buffName!] = output.Amount;
                    break;
            }

            string ckSelfIndv = "";
            if (buff.CkSelfIndv.Length != 0)
                ckSelfIndv = $" for {string.Join(" & ", buff.CkSelfIndv.Select(trait => $"{traitService.GetTrait(trait)}"))}";
            
            var zippedOutput = $"**{baseAction} [{buffName}{ckSelfIndv}]** "
                               + (string.IsNullOrWhiteSpace(amount) ? "" : $"{amountPreposition} " + $"**{amount}**")
                               + (string.IsNullOrWhiteSpace(limits) ? "" : $" ({limits})");

            if (function.Tvals.Length != 0)
                extra.Add(
                    "Only applies for "
                    + string.Join(" & ", function.Tvals.Select(tvals => traitService.GetTrait(tvals)))
                    + " targets"
                );
            
            if (function.QuestTvals.Length != 0)
                extra.Add(
                    "Only applies on "
                      + string.Join(" & ", function.QuestTvals.Select(questTvals => traitService.GetTrait(questTvals)))
                      + " field"
                );

            return (zippedOutput.Trim(), extraStats, extra.ToArray());
        }
    }
}