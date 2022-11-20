using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Commons.Structures;
using Qmmands;
using Qmmands.Text;
using Qmmands.Text.Default;
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
            var _ = Bot.VersionHash;
            var services = new ServiceCollection()
                .AddLogging()
                .AddTextCommandService()
                .BuildServiceProvider();
            var commandService = services.GetRequiredService<ICommandService>();
            DefaultTextSetup.Initialize(commandService);
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var name = assembly.FullName!;
                if (name.StartsWith("Pepper") && !name.Contains("Test", StringComparison.OrdinalIgnoreCase))
                {
                    commandService.AddModules(assembly, Bot.DownlevelAttributes);
                }
            }

            return type switch
            {
                Data.Command => commandService.EnumerateTextCommands().Select(command => new object[] { command }),
                Data.Parameter => commandService.EnumerateTextCommands()
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
        public void AllCommandsFullyDescribed(ITextCommand command)
        {
            Assert.NotEqual("", command.Description);
            Assert.NotNull(command.Description);
        }

        [Theory]
        [CommandData(CommandDataAttribute.Data.Parameter)]
        public void AllParametersFullyDescribed(ITextParameter parameter)
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