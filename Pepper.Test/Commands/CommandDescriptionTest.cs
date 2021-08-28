using System;
using Qmmands;
using Xunit;

namespace Pepper.Test.Commands
{
    public class CommandDescriptionTest
    {
        [Fact]
        public void AllCommandsFullyDescribed()
        {
            var commandService = new CommandService();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                if (assembly.FullName.StartsWith("Pepper"))
                    commandService.AddModules(assembly);

            foreach (var command in commandService.GetAllCommands())
            {
                Assert.NotEqual("", command.Description);
                Assert.NotNull(command.Description);
                foreach (var parameter in command.Parameters)
                {
                    Assert.NotEqual("", parameter.Description);
                    Assert.NotNull(parameter.Description);
                }
            }
        }
    }
}