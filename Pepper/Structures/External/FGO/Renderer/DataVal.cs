using System;
using System.Collections.Generic;
using System.Linq;

namespace Pepper.Structures.External.FGO.Renderer
{
    internal class FunctionToDatavalMapping
    {
        public FunctionType[] Conditions = Array.Empty<FunctionType>();
        public object[] Guides = Array.Empty<object>();
    }
    internal struct SpecialParsingInstruction
    {
        public int ArgumentCount;
        public Func<string[], (string, string)> Parser;
    }

    public static class DataVal
    {
        private static readonly FunctionType[] EventFunctions =
        {
            FunctionType.EventPointUp,
            FunctionType.EventPointRateUp,
            FunctionType.DropUp,
            FunctionType.EventDropRateUp,
            FunctionType.EnemyEncountCopyRateUp,
            FunctionType.EnemyEncountRateUp
        };
        
        
        private static readonly FunctionToDatavalMapping[] ParsingInstructions =
        {
            new()
            {
                Conditions = new []
                {
                    FunctionType.DamageNPRare,
                    FunctionType.DamageNPIndividual,
                    FunctionType.DamageNPIndividualSum,
                    FunctionType.DamageNPStateIndividual,
                    FunctionType.DamageNPStateIndividualFix
                },
                Guides = new object[] { "Rate", "Value", "Target", "Correction" }
            },
            new()
            {
                Conditions = new []
                {
                    FunctionType.AddState,
                    FunctionType.AddStateShort
                },
                Guides = new object[] { "Rate", "Turn", "Count", "Value", "UseRate", "Value2" }
            },
            new()
            {
                Conditions = new []
                {
                    FunctionType.SubState
                },
                Guides = new object[] { "Rate", "Value", "Value2" }
            },
            new ()
            {
                Conditions = new []
                {
                    FunctionType.EnemyProbDown
                },
                Guides = new object[] { "Individuality", "RateCount", "EventId" }
            },
            new()
            {
                Conditions = new []
                {
                    FunctionType.FriendPointUp,
                    FunctionType.FriendPointUpDuplicate
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
                    FunctionType.ClassDropUp
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
                    FunctionType.ServantFriendshipUp,
                    FunctionType.UserEquipExpUp,
                    FunctionType.ExpUp,
                    FunctionType.QPDropUp,
                    FunctionType.QPUp
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
        
        public static Dictionary<string, string> Parse(string raw, int functionType)
        {
            try
            {
                var parsed = NestedSplitting(raw);
                var numericParsed = parsed.Where(value => long.TryParse(value, out _)).ToArray();
                var output = parsed
                    .Where(value => !long.TryParse(value, out _))
                    .Where(value => value.Contains(':'))
                    .Select(value => value.Split(':'))
                    .ToDictionary(
                        value => value[0],
                        value => value[1]
                    );

                if (!Enum.IsDefined(typeof(FunctionType), functionType))
                    throw new ArgumentException($"{functionType} is not a valid function type!");

                var instructions = ParsingInstructions.FirstOrDefault(mapping => mapping.Conditions.Contains((FunctionType) functionType));
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

                return output;
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