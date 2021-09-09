using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qmmands;

namespace Pepper.Structures.Commands
{
    public class ArgumentParser : IArgumentParser
    {
        private static readonly char Quote = '"';

        public ValueTask<ArgumentParserResult> ParseAsync(Qmmands.CommandContext context)
        {
            var command = context.Command;
            var rawArguments = context.RawArguments.TrimStart();
            var parameters = new Dictionary<Parameter, object>();

            // Initialize to default values.
            foreach (var param in command.Parameters)
                parameters[param] = param.DefaultValue ?? "";

            // sort parameters into two types : with and without flags
            var flagParameters = new Dictionary<string, Parameter>();
            var nonFlagParameters = new LinkedList<Parameter>();
            foreach (var param in context.Command.Parameters)
            {
                var flagAttribute = param.Attributes.FirstOrDefault(attrib => attrib is FlagAttribute);
                if (flagAttribute == null)
                    nonFlagParameters.AddLast(param);
                else
                    foreach (var flag in ((FlagAttribute) flagAttribute).Flags)
                        flagParameters.Add(flag, param);
            }

            // sort dictionary by key.
            // this is done to prefer longer flags over shorter colliding ones (/flag1 over /f)
            flagParameters = flagParameters
                .OrderByDescending(pair => pair.Key, StringComparer.InvariantCultureIgnoreCase)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
            
            // sort parameters into two types : matches flags and doesn't
            HashSet<string> flagArguments = new(); List<string> remainingArguments = new();
            foreach (var argument in rawArguments.SmartSplit(Quote))
                if (flagParameters.Any(f => argument.StartsWith(f.Key)))
                    flagArguments.Add(argument);
                else 
                    remainingArguments.Add(argument);

            
            Dictionary<Parameter, object> flagParameterValues = new();
            foreach (var (flagPrefix, parameter) in flagParameters)
            {
                List<string> matchingArguments = new();
                if (parameter.IsMultiple)
                {
                    foreach (var argument in flagArguments)
                        if (argument.StartsWith(flagPrefix))
                        {
                            matchingArguments.Add(argument);
                            flagArguments.Remove(argument);
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
                    if (parameter.Type == typeof(bool) && parameter.Attributes.Any(attrib => attrib is FlagAttribute))
                        arg = bool.TrueString;
                    else
                    {
                        var passingArgument = arg[flagPrefix.Length..] ?? "";
                        arg = passingArgument.StartsWith(Quote) && passingArgument.EndsWith(Quote)
                            ? passingArgument[1..^1] ?? ""
                            : passingArgument;
                    }

                    matchingArguments[i] = arg;
                }

                if (parameter.IsMultiple)
                {
                    // in case this is a multiple-value parameter, merge previous parsed values
                    if (flagParameterValues.TryGetValue(parameter, out var list))
                        ((List<string>) list).AddRange(matchingArguments);
                    else
                        flagParameterValues[parameter] = matchingArguments;
                }
                else 
                    if (matchingArguments.Count != 0) flagParameterValues[parameter] = matchingArguments[0];
            }


            foreach (var (leftoverArgument, index) in remainingArguments.Select((arg, i) => (arg, i)))
            {
                if (!nonFlagParameters.Any()) break;

                // take the first parameter and remove it from the queue
                var param = nonFlagParameters.First!.Value;
                nonFlagParameters.RemoveFirst();

                if (param.IsRemainder)
                {
                    parameters[param] = string.Join(' ', remainingArguments.Skip(index));
                    break;
                }

                if (param.IsMultiple)
                {
                    parameters[param] = string.Join(' ', remainingArguments.Skip(index));
                    break;
                }
                    

                parameters[param] = leftoverArgument;
            }
            
            foreach (var (parameter, value) in flagParameterValues) parameters[parameter] = value;
            return new DefaultArgumentParserResult(command, parameters);
        }
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