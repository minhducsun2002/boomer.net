using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Structures;
using Serilog;

namespace Pepper
{
    static partial class Pepper
    {
        private static readonly Client Client = new Client();
        private static Configuration configuration = new Configuration();
        private static IServiceProvider serviceProvider = new ServiceCollection().BuildServiceProvider();
        private static Type[] serviceTypes = {};
        
        // inspect embedded assembly metadata 
        public static readonly string VersionHash = typeof(Pepper)
            .Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(a => a.Key == "GitHash")?.Value ?? "unknown";

        public static async Task<int> Main(string[] args)
        {
            PreInitialize();
            var init = InitializeServices();
            await init;
            if (init.Status == TaskStatus.Faulted)
            {
                Log.Fatal(init.Exception, "An exception occurred during services initialization.");
                Environment.Exit(1);
            }
            Log.Debug("We are done with all initializations. Connecting to Discord...");
                
            await Client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DISCORD_TOKEN"));
            await Client.StartAsync();
            await Task.Delay(-1);
            return 0;
        }
    }
}
