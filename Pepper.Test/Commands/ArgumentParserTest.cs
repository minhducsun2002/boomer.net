using System;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Commons.Structures;
using Pepper.Structures;
using Pepper.Test.Commands.Mock;
using Qmmands;
using Qmmands.Text;
using Qmmands.Text.Default;
using Xunit;

namespace Pepper.Test.Commands
{
    public class ArgumentParserTest
    {
        private readonly ICommandService commandService;
        private readonly ServiceProvider serviceProvider;
        public ArgumentParserTest()
        {
            serviceProvider = new ServiceCollection()
                .AddOptions()
                .AddLogging()
                .AddTextCommandService()
                .BuildServiceProvider();
            commandService = serviceProvider.GetRequiredService<ICommandService>();
            DefaultTextSetup.Initialize(commandService);

            var parserProvider = (DefaultArgumentParserProvider) serviceProvider.GetRequiredService<IArgumentParserProvider>();
            parserProvider.Add(new ArgumentParser());
            parserProvider.SetDefaultParser(typeof(ArgumentParser));



            commandService.AddModule(typeof(TestCommandModule).GetTypeInfo());
        }

        [Fact]
        public async Task TestHandleQuote()
        {
            const string arg1 = "woke", arg2 = "woke 2";
            var context = new DefaultTextCommandContext(serviceProvider, CultureInfo.InvariantCulture, default)
            {
                InputString = $"{nameof(TestCommandModule.Exec1)} {arg1} \"{arg2}\"".AsMemory()
            };
            var r = (TestCommandModule.ExecResult<string>) await commandService.ExecuteAsync(context);
            Assert.Equal(r.Arguments[0], arg1);
            Assert.Equal(r.Arguments[1], arg2);

        }

        [Fact]
        public async Task TestHandleFlag()
        {
            const int arg1 = 2, arg2 = 3;
            var context = new DefaultTextCommandContext(serviceProvider, CultureInfo.InvariantCulture, default)
            {
                InputString = $"{nameof(TestCommandModule.Exec2)} {arg1} /f={arg2}".AsMemory()
            };
            var r = (TestCommandModule.ExecResult<int>) await commandService.ExecuteAsync(context);
            // flag is arg2
            Assert.Equal(r.Arguments[0], arg2);
            Assert.Equal(r.Arguments[1], arg1);
        }
    }
}