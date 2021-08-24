using System;
using System.Collections.Generic;
using System.Linq;
using FgoExportedConstants;

namespace Pepper.Structures.External.FGO.Renderer
{
    internal class FunctionToDatavalMapping
    {
        public FuncList.TYPE[] Conditions = Array.Empty<FuncList.TYPE>();
        public object[] Guides = Array.Empty<object>();
    }
    internal struct SpecialParsingInstruction
    {
        public int ArgumentCount;
        public Func<string[], (string, string)> Parser;
    }

    public static class DataValParser
    {
        private static readonly FuncList.TYPE[] EventFunctions =
        {
            FuncList.TYPE.EVENT_POINT_UP,
            FuncList.TYPE.EVENT_POINT_RATE_UP,
            FuncList.TYPE.EVENT_DROP_UP,
            FuncList.TYPE.DROP_UP,
            FuncList.TYPE.EVENT_DROP_RATE_UP,
            FuncList.TYPE.ENEMY_ENCOUNT_COPY_RATE_UP,
            FuncList.TYPE.ENEMY_ENCOUNT_RATE_UP
        };
        
        
        private static readonly FunctionToDatavalMapping[] ParsingInstructions =
        {
            new()
            {
                Conditions = new []
                {
                    FuncList.TYPE.DAMAGE_NP_RARE,
                    FuncList.TYPE.DAMAGE_NP_INDIVIDUAL,
                    FuncList.TYPE.DAMAGE_NP_INDIVIDUAL_SUM,
                    FuncList.TYPE.DAMAGE_NP_STATE_INDIVIDUAL,
                    FuncList.TYPE.DAMAGE_NP_STATE_INDIVIDUAL_FIX
                },
                Guides = new object[] { "Rate", "Value", "Target", "Correction" }
            },
            new()
            {
                Conditions = new []
                {
                    FuncList.TYPE.ADD_STATE,
                    FuncList.TYPE.ADD_STATE_SHORT
                },
                Guides = new object[] { "Rate", "Turn", "Count", "Value", "UseRate", "Value2" }
            },
            new()
            {
                Conditions = new []
                {
                    FuncList.TYPE.SUB_STATE
                },
                Guides = new object[] { "Rate", "Value", "Value2" }
            },
            new ()
            {
                Conditions = new []
                {
                    FuncList.TYPE.ENEMY_PROB_DOWN
                },
                Guides = new object[] { "Individuality", "RateCount", "EventId" }
            },
            new()
            {
                Conditions = new []
                {
                    FuncList.TYPE.FRIEND_POINT_UP,
                    FuncList.TYPE.FRIEND_POINT_UP_DUPLICATE
                },
                Guides = new object[] { "AddCount" }
            },
            new()
            {
                Conditions = EventFunctions,
                Guides = new object[] { 
                    "Individuality",
                    new SpecialParsingInstruction
                    {
                        ArgumentCount = 2,
                        Parser = DependentDatavalParsingDelegate.Delegate1
                    },
                    "EventId"
                }
            },
            new()
            {
                Conditions = new []
                {
                    FuncList.TYPE.CLASS_DROP_UP
                },
                Guides = new object[]
                {
                    new SpecialParsingInstruction
                    {
                        ArgumentCount = 2,
                        Parser = DependentDatavalParsingDelegate.Delegate1
                    },
                    "EventId"
                }
            },
            new()
            {
                Conditions = new []
                {
                    FuncList.TYPE.SERVANT_FRIENDSHIP_UP,
                    FuncList.TYPE.USER_EQUIP_EXP_UP,
                    FuncList.TYPE.EXP_UP,
                    FuncList.TYPE.QP_DROP_UP,
                    FuncList.TYPE.QP_UP
                },
                Guides = new object[]
                {
                    new SpecialParsingInstruction
                    {
                        ArgumentCount = 2,
                        Parser = DependentDatavalParsingDelegate.Delegate1
                    },
                    "Individuality"
                }
            },
            new()
            {
                Conditions = new []
                {
                    FuncList.TYPE.TRANSFORM_SERVANT
                },
                Guides = new object[] { "Rate", "Value", "Target", "SetLimitCount" }
            }
        };
        
        private static string[] NestedSplitting(string raw)
        {
            while (raw.StartsWith('[') && raw.EndsWith(']')) raw = raw[1..^1];
            var @out = new List<string>();
            int depth = 0;
            string current = "";
            foreach (var character in raw)
            {
                if (character == ',' && (depth == 0))
                {
                    @out.Add(current);
                    current = "";
                    continue;
                }

                if (character == '[') depth++;
                if (character == ']') depth--;
                current += character;
            }

            @out.Add(current);
            return @out.ToArray();
        }
        
        private static readonly Type FunctionType = typeof(FuncList.TYPE);
        public static Entities.DataVal Parse(string raw, int functionType)
        {
            try
            {
                var parsed = NestedSplitting(raw);
                var numericParsed = parsed.Where(value => long.TryParse(value, out _)).ToArray();
                var output = parsed
                    .Where(value => !long.TryParse(value, out _))
                    .Where(value => value.Contains(':'))
                    .Select(value => value.Split(':'))
                    .GroupBy(value => value[0])
                    .ToDictionary(
                        value => value.Key,
                        value => value.First()[1]
                    );

                if (!Enum.IsDefined(FunctionType, functionType))
                    throw new ArgumentException($"{functionType} is not a valid function type!");

                var instructions = ParsingInstructions.FirstOrDefault(mapping => mapping.Conditions.Contains((FuncList.TYPE) functionType));
                object[] guides = instructions == null ? new object[] {"Rate", "Value", "Target"} : instructions.Guides;

                var currentPosition = 0;
                foreach (var key in guides)
                {
                    if (currentPosition >= numericParsed.Length) break;
                    switch (key)
                    {
                        case string normalKey:
                            output[normalKey] = numericParsed[currentPosition];
                            currentPosition++;
                            break;
                        case SpecialParsingInstruction specialKey:
                            var count = specialKey.ArgumentCount;
                            var arguments = numericParsed[currentPosition..(currentPosition + count)];
                            currentPosition += count;
                            var (parsedKey, parsedValue) = specialKey.Parser(arguments);
                            output[parsedKey] = parsedValue;
                            break;
                        default:
                            throw new ArgumentException(
                                $"Parsing failed : unknown instruction type (type was {key.GetType().FullName})");
                    }
                }

                return new Entities.DataVal(output);
            }
            catch (Exception e)
            {
                e.Data.Add("Function type", $"{functionType}");
                e.Data.Add("Raw argument", $"\"{raw}\"");
                throw;
            }
        }
    }

    internal static class DependentDatavalParsingDelegate
    {
        public static (string, string) Delegate1(string[] values)
        {
            return (
                int.Parse(values[0]) switch
                {
                    1 => "AddCount",
                    2 => "RateCount",
                    _ => throw new ArgumentException($"Expression type ${values[0]} is not handled!")
                },
                values[1]
            );
        }
    }
}