using System.Reflection;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using dotenv.net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pepper.Commons;
using Pepper.Commons.Structures;
using Pepper.Frontends.MaimaiStatistics.Database.ProgressRecordProvider;

DotEnv.Load();
var webhookLog = Environment.GetEnvironmentVariable("PEPPER_DISCORD_WEBHOOK_LOG");

var hostBuilder = new HostBuilder()
    .UseLogging(webhookLog)
    .UseEnvironmentVariables()
    .UseDefaultServices()
    .ConfigureServices((_, services) =>
    {
        services.AddDbContextPool<IProgressRecordProvider, MariaDbProgressRecordProvider>(builder =>
        {
            var connectionString = Environment.GetEnvironmentVariable("MARIADB_CONNECTION_STRING_MAIMAI_DUMP")!;
            builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }, 16);
    })
    .ConfigureDiscordBot<Bot>((context, bot) =>
    {
        bot.Token = context.Configuration["DISCORD_TOKEN"];
        bot.Prefixes = new[] { "s!" };
        bot.Intents |= GatewayIntents.DirectMessages;
        bot.ServiceAssemblies = new[]
        {
            Assembly.GetEntryAssembly()!, typeof(Service).Assembly
        };
    })
    .UseDefaultServiceProvider(option => option.ValidateOnBuild = true);

await hostBuilder.RunConsoleAsync();