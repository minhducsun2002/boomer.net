using System.Reflection;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using dotenv.net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pepper.Commons;
using Pepper.Commons.Maimai;
using Pepper.Commons.Structures;
using Pepper.Frontends.Maimai.Database;
using Pepper.Frontends.Maimai.Database.MaimaiDxNetCookieProviders;

DotEnv.Load();
var webhookLog = Environment.GetEnvironmentVariable("PEPPER_DISCORD_WEBHOOK_LOG");

var hostBuilder = new HostBuilder()
    .UseLogging(webhookLog)
    .UseEnvironmentVariables()
    .ConfigureServices((context, services) =>
    {
        services.AddDbContextPool<IMaimaiDxNetCookieProvider, MariaDbMaimaiDxNetCookieProvider>(builder =>
        {
            var connectionString = Environment.GetEnvironmentVariable("MARIADB_CONNECTION_STRING")!;
            builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }, 16);
        services.AddDbContextPool<MaimaiDataDbContext>(builder =>
        {
            var connectionString = Environment.GetEnvironmentVariable("MARIADB_CONNECTION_STRING_MAIMAI")!;
            builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }, 16);
        services.AddSingleton<HttpClient>();
    })
    .ConfigureDiscordBot<Bot>((context, bot) =>
    {
        bot.Token = context.Configuration["DISCORD_TOKEN"];
        bot.Prefixes = new [] { "b!", "o!" };
        bot.Intents |= GatewayIntent.DirectMessages;
        bot.ServiceAssemblies = new[]
        {
            Assembly.GetEntryAssembly()!, typeof(Service).Assembly
        };
    })
    .UseDefaultServiceProvider(options => options.ValidateOnBuild = true);
    
await hostBuilder.RunConsoleAsync();