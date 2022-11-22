using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Commons.Commands.General;
using Pepper.Commons.Structures;
using Pepper.Frontends.Maimai.Commands;
using Pepper.Frontends.Osu.Commands;
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
            var assemblies = new[]
            {
                typeof(OsuCommand).Assembly,
                typeof(MaimaiCommand).Assembly,
            };
            var bots = assemblies.Select(a =>
            {
                var services = new ServiceCollection()
                    .AddLogging()
                    .AddTextCommandService()
                    .BuildServiceProvider();
                var commandService = services.GetRequiredService<ICommandService>();
                DefaultTextSetup.Initialize(commandService);
                commandService.AddModules(typeof(GeneralCommand).Assembly);
                commandService.AddModules(a);
                return (services, commandService, commands: commandService.EnumerateTextCommands());
            });

            return type switch
            {
                Data.Command => bots.SelectMany(p => p.commands).Select(command => new object[] { command }),
                Data.Parameter => bots
                    .SelectMany(p => p.commands)
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