using System;
using System.Linq;
using Pepper.Structures.Commands;
using Qmmands;
using Xunit;
using Xunit.Abstractions;

namespace Pepper.Test.Commands
{
    public class CommandDescriptionTest
    {
        private readonly ITestOutputHelper testOutputHelper;

        public CommandDescriptionTest(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
            // ensure the assembly will always be loaded by referring to it
            var _ = Pepper.VersionHash;
        }

        [Fact]
        public void AllCommandsFullyDescribed()
        {
            var commandService = new CommandService();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                if (assembly.FullName.StartsWith("Pepper"))
                    commandService.AddModules(assembly, null, Pepper.DownlevelAttributes);

            foreach (var command in commandService.GetAllCommands())
            {
                try
                {
                    Assert.NotEqual("", command.Description);
                    Assert.NotNull(command.Description);                    
                }
                catch
                {
                    var category = command.Attributes.OfType<CategoryAttribute>().FirstOrDefault()
                        ?.Category;
                    testOutputHelper.WriteLine($"Command : {command.Name}" + (category == null ? "" : $", category : {category}"));
                    throw;
                }
                
                foreach (var parameter in command.Parameters)
                    try
                    {
                        Assert.NotEqual("", parameter.Description);
                        Assert.NotNull(parameter.Description);
                    }
                    catch (Exception e)
                    {
                        testOutputHelper.WriteLine($"Command : {command.Name}, parameter : {parameter.Name}, type : {parameter.Type.FullName}");
                        throw;
                    }
                
            }
        }
    }
}