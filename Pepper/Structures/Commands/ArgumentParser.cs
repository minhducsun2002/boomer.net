using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pepper.Commons.Extensions;
using Qmmands;
using Qmmands.Text;
using Qmmands.Text.Default;
using Qommon;

namespace Pepper.Structures.Commands
{
    public class ArgumentParser : IArgumentParser
    {
        private static readonly char Quote = '"';

        public void Validate(ITextCommand command) { }

        public ValueTask<IArgumentParserResult> ParseAsync(ITextCommandContext context)
        {
            var command = context.Command!;
            var rawArguments = context.RawArgumentString.TrimStart();
            var parameters = new Dictionary<ITextParameter, List<string>>();

            // sort parameters into two types : with and without flags
            var flagParameters = new Dictionary<string, ITextParameter>();
            var positionalParameters = new LinkedList<ITextParameter>();
            foreach (var param in command.Parameters)
            {
                var flagAttribute = param.CustomAttributes.OfType<FlagAttribute>().FirstOrDefault();
                if (flagAttribute == null)
                {
                    positionalParameters.AddLast(param);
                }
                else
                {
                    foreach (var flag in flagAttribute.Flags)
                    {
                        flagParameters.Add(flag, param);
                    }
                }
            }

            // sort dictionary by key.
            // this is done to prefer longer flags over shorter colliding ones (/flag1 over /f)
            flagParameters = flagParameters
                .OrderByDescending(pair => pair.Key, StringComparer.InvariantCultureIgnoreCase)
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            // sort parameters into two types : matches flags and doesn't
            HashSet<string> flagArguments = new(); List<string> positionalArguments = new();
            foreach (var argument in rawArguments.ToString().SplitWithQuotes(Quote))
            {
                if (flagParameters.Any(f => argument.StartsWith(f.Key)))
                {
                    flagArguments.Add(argument);
                }
                else
                {
                    positionalArguments.Add(argument);
                }
            }

            Dictionary<ITextParameter, object> flagParameterValues = new();
            foreach (var (flagPrefix, parameter) in flagParameters)
            {
                List<string> matchingArguments = new();
                if (parameter.GetTypeInformation().IsEnumerable)
                {
                    foreach (var argument in flagArguments)
                    {
                        if (argument.StartsWith(flagPrefix))
                        {
                            matchingArguments.Add(argument);
                            flagArguments.Remove(argument);
                        }
                    }
                }
                else
                {
                    // only take the last matching argument if this is a single value parameter
                    var arg = flagArguments.LastOrDefault(arg => arg.StartsWith(flagPrefix));
                    if (arg != default)
                    {
                        matchingArguments.Add(arg);
                        flagArguments.Remove(arg);
                    }
                }

                for (var i = 0; i < matchingArguments.Count; i++)
                {
                    var arg = matchingArguments[i];
                    if (parameter.GetTypeInformation().ActualType == typeof(bool) && parameter.CustomAttributes.OfType<FlagAttribute>().Any())
                    {
                        arg = bool.TrueString;
                    }
                    else
                    {
                        var passingArgument = arg[flagPrefix.Length..] ?? "";
                        arg = passingArgument.StartsWith(Quote) && passingArgument.EndsWith(Quote)
                            ? passingArgument[1..^1] ?? ""
                            : passingArgument;
                    }

                    matchingArguments[i] = arg;
                }

                if (parameter.GetTypeInformation().IsEnumerable)
                {
                    // in case this is a multiple-value parameter, merge previous parsed values
                    if (flagParameterValues.TryGetValue(parameter, out var list))
                    {
                        ((List<string>) list).AddRange(matchingArguments);
                    }
                    else
                    {
                        flagParameterValues[parameter] = matchingArguments;
                    }
                }
                else
                    if (matchingArguments.Count != 0)
                {
                    flagParameterValues[parameter] = matchingArguments[0];
                }
            }


            foreach (var (positionalArgument, index) in positionalArguments.Select((arg, i) => (arg, i)))
            {
                if (!positionalParameters.Any())
                {
                    break;
                }

                // take the first parameter and remove it from the queue
                var param = positionalParameters.First!.Value;
                positionalParameters.RemoveFirst();

                if (param is IPositionalParameter { IsRemainder: true })
                {
                    parameters[param] = positionalArguments.Skip(index).ToList();
                    break;
                }

                if (param.GetTypeInformation().IsEnumerable)
                {
                    parameters[param] = positionalArguments.Skip(index).ToList();
                    break;
                }


                parameters[param] = new List<string> { positionalArgument };
            }

            foreach (var (parameter, value) in flagParameterValues)
            {
                parameters[parameter] = value switch
                {
                    List<string> v => v,
                    string v => new List<string> { v },
                    _ => throw new ArgumentException($"parameter \"{parameter.Name}\" parsed value is of unexpected type {value.GetType()}")
                };
            }

            // Initialize to default value if needed.
            foreach (var param in command.Parameters)
            {
                if (!parameters.ContainsKey(param) && param.GetTypeInformation().IsOptional)
                {
                    parameters[param] = new List<string> { "" };
                }
            }

            var result = parameters
                .ToDictionary(
                    p => p.Key as IParameter,
                    p => new MultiString(
                        p.Value
                            .Select(v => new ReadOnlyMemory<char>(v.ToCharArray()))
                            .ToList()
                    )
                );

            return new ClassicArgumentParserResult(result);
        }

        public bool SupportsOptionalParameters => true;
    }

    public class FlagAttribute : Attribute
    {
        public string[] Flags { get; }

        public FlagAttribute(params string[] flags)
        {
            Flags = flags;
        }
    }
}