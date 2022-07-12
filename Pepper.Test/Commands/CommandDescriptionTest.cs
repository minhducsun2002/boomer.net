using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Qmmands;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Pepper.Test.Commands
{
    internal class CommandDataAttribute : DataAttribute
    {
        public enum Data
        {
            Command = 1,
            Parameter = 2
        }

        private readonly Data type;
        public CommandDataAttribute(Data type) => this.type = type;
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            var _ = Structures.Bot.VersionHash;
            var commandService = new CommandService();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName.StartsWith("Pepper"))
                {
                    commandService.AddModules(assembly, null, Structures.Bot.DownlevelAttributes);
                }
            }

            return type switch
            {
                Data.Command => commandService.GetAllCommands().Select(command => new object[] { command }),
                Data.Parameter => commandService.GetAllCommands()
                    .SelectMany(command => command.Parameters)
                    .Select(parameter => new object[] { parameter }),
                _ => throw new ArgumentException($"Invalid data type for {nameof(type)}")
            };
        }
    }

    public class CommandDescriptionTest
    {
        private readonly ITestOutputHelper testOutputHelper;

        public CommandDescriptionTest(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Theory]
        [CommandData(CommandDataAttribute.Data.Command)]
        public void AllCommandsFullyDescribed(Command command)
        {
            Assert.NotEqual("", command.Description);
            Assert.NotNull(command.Description);
        }

        [Theory]
        [CommandData(CommandDataAttribute.Data.Parameter)]
        public void AllParametersFullyDescribed(Parameter parameter)
        {
            try
            {
                Assert.NotEqual("", parameter.Description);
                Assert.NotNull(parameter.Description);
            }
            catch
            {
                testOutputHelper.WriteLine($"Command in check : {parameter.Command.Name}");
                throw;
            }
        }
    }
}